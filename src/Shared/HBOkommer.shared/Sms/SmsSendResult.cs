namespace HBOkommer.Shared.Sms;

public sealed class SmsSendResult
{
    public required bool Success { get; init; }
    public string? ProviderMessageId { get; init; }

    // Standardiserte reasonCodes (timeout/rejected/unhandled) brukes senere i Trinn 6.
    public required string ReasonCode { get; init; }
}
