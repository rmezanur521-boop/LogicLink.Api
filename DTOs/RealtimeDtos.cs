namespace LogicLink.Api.DTOs;

public record ParticipantDto(string ConnectionId, string DisplayName, string Color);

public record JoinCircuitResult(string DisplayName, string Color, IReadOnlyList<ParticipantDto> Participants);

public record CursorPositionDto(string ConnectionId, string DisplayName, double X, double Y);

public record MoveGateRequest(Guid GateId, double X, double Y, double Rotation);