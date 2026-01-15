using System.Text.Json.Serialization;

namespace HboKommer.Shared.Contracts;

/// <summary>
/// Event Contract V1 - minimal identitet + tidspunkt.
/// Denne kontrakten er bevisst liten i pilot.
/// </summary>
public sealed class VisitStartedEventV1
{
    // Krav fra Konfigurasjonsmodell v1 (identitetsmodell)
    [JsonPropertyName("eventId")]
    public required string EventId { get; init; }

    [JsonPropertyName("eventType")]
    public required string EventType { get; init; } // forventet: "VISIT_STARTED"

    [JsonPropertyName("municipalityId")]
    public required string MunicipalityId { get; init; }

    [JsonPropertyName("unitId")]
    public required string UnitId { get; init; }

    [JsonPropertyName("subjectRef")]
    public required string SubjectRef { get; init; }

    [JsonPropertyName("occurredAtUtc")]
    public required DateTimeOffset OccurredAtUtc { get; init; }

    // Nyttig i mottak, men ikke et krav i identitetsmodellen:
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0";
}
