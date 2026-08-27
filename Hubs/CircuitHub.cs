using LogicLink.Api.DTOs;
using LogicLink.Api.Realtime;
using LogicLink.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LogicLink.Api.Hubs;

public class CircuitHub : Hub
{
    private readonly PresenceTracker _presence;
    private readonly ICircuitService _circuitService;

    public CircuitHub(PresenceTracker presence, ICircuitService circuitService)
    {
        _presence = presence;
        _circuitService = circuitService;
    }

    public async Task<JoinCircuitResult> JoinCircuit(Guid circuitId, string requestedName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(circuitId));

        var self = _presence.Join(circuitId, Context.ConnectionId, requestedName);
        var participants = _presence.GetParticipants(circuitId);

        await Clients.OthersInGroup(GroupName(circuitId)).SendAsync("UserJoined", self);

        return new JoinCircuitResult(self.DisplayName, self.Color, participants);
    }

    public async Task MoveCursor(Guid circuitId, double x, double y)
    {
        var participants = _presence.GetParticipants(circuitId);
        var self = participants.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
        if (self is null) return;

        var payload = new CursorPositionDto(self.ConnectionId, self.DisplayName, x, y);
        await Clients.OthersInGroup(GroupName(circuitId)).SendAsync("CursorMoved", payload);
    }

    public async Task<GateDto> AddGate(Guid circuitId, GateDto gate)
    {
        var saved = await _circuitService.AddGateAsync(circuitId, gate);
        await Clients.OthersInGroup(GroupName(circuitId)).SendAsync("GateAdded", saved);
        return saved;
    }

    public async Task MoveGate(Guid circuitId, MoveGateRequest request)
    {
        var updated = await _circuitService.MoveGateAsync(circuitId, request.GateId, request.X, request.Y, request.Rotation);
        if (updated is null) return;

        await Clients.OthersInGroup(GroupName(circuitId)).SendAsync("GateMoved", updated);
    }

    public async Task DeleteGate(Guid circuitId, Guid gateId)
    {
        var deleted = await _circuitService.DeleteGateAsync(circuitId, gateId);
        if (!deleted) return;

        await Clients.OthersInGroup(GroupName(circuitId)).SendAsync("GateDeleted", gateId);
    }

    public async Task<WireDto> AddWire(Guid circuitId, WireDto wire)
    {
        var saved = await _circuitService.AddWireAsync(circuitId, wire);
        await Clients.OthersInGroup(GroupName(circuitId)).SendAsync("WireAdded", saved);
        return saved;
    }

    public async Task DeleteWire(Guid circuitId, Guid wireId)
    {
        var deleted = await _circuitService.DeleteWireAsync(circuitId, wireId);
        if (!deleted) return;

        await Clients.OthersInGroup(GroupName(circuitId)).SendAsync("WireDeleted", wireId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var result = _presence.Leave(Context.ConnectionId);
        if (result is { } left)
        {
            await Clients.Group(GroupName(left.CircuitId))
                .SendAsync("UserLeft", Context.ConnectionId, left.Remaining);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string GroupName(Guid circuitId) => $"circuit-{circuitId}";
}