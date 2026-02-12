namespace HBOkommer.Shared.Sms;

public sealed class SmsSendRequest
{
    public required string MunicipalityId { get; init; }
    public required string EventId { get; init; }
    public required string ToPhoneE164 { get; init; }

    // Nøytral tekst (ingen helseopplysninger) – i pilot kan vi hardkode tekst senere i Trinn 9.
    public required string MessageText { get; init; }

    // For sporbarhet hos leverandør (valgfritt, men nyttig)
    public string? CorrelationId { get; init; }
}
