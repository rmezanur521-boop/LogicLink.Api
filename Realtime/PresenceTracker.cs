using System.Collections.Concurrent;
using LogicLink.Api.DTOs;

namespace LogicLink.Api.Realtime;

public class PresenceTracker
{
    private static readonly string[] Palette =
        { "#C1793F", "#4FB8A6", "#8B6FD6", "#D6A24F", "#5B9BD6" };

    // circuitId -> (connectionId -> participant)
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, ParticipantDto>> _rooms = new();

    private readonly ConcurrentDictionary<string, Guid> _connectionToCircuit = new();

    public ParticipantDto Join(Guid circuitId, string connectionId, string requestedName)
    {
        var room = _rooms.GetOrAdd(circuitId, _ => new ConcurrentDictionary<string, ParticipantDto>());

        var displayName = ResolveUniqueName(room, requestedName);
        var color = Palette[room.Count % Palette.Length];
        var participant = new ParticipantDto(connectionId, displayName, color);

        room[connectionId] = participant;
        _connectionToCircuit[connectionId] = circuitId;

        return participant;
    }

    public (Guid CircuitId, IReadOnlyList<ParticipantDto> Remaining)? Leave(string connectionId)
    {
        if (!_connectionToCircuit.TryRemove(connectionId, out var circuitId))
            return null;

        if (_rooms.TryGetValue(circuitId, out var room))
        {
            room.TryRemove(connectionId, out _);
            if (room.IsEmpty) _rooms.TryRemove(circuitId, out _);
            return (circuitId, room.Values.ToList());
        }

        return (circuitId, Array.Empty<ParticipantDto>());
    }

    public IReadOnlyList<ParticipantDto> GetParticipants(Guid circuitId) =>
        _rooms.TryGetValue(circuitId, out var room) ? room.Values.ToList() : Array.Empty<ParticipantDto>();

    // "John", "John" come -> "John 2", "John 3"...
    private static string ResolveUniqueName(ConcurrentDictionary<string, ParticipantDto> room, string requestedName)
    {
        var baseName = string.IsNullOrWhiteSpace(requestedName) ? "Guest" : requestedName.Trim();
        var existingNames = room.Values.Select(p => p.DisplayName).ToHashSet();

        if (!existingNames.Contains(baseName)) return baseName;

        var counter = 2;
        while (existingNames.Contains($"{baseName} {counter}")) counter++;
        return $"{baseName} {counter}";
    }
}