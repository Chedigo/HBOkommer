namespace HBOkommer.Shared.Policy;

public sealed class PolicyDecision
{
    public bool Eligible { get; init; }
    public required string ReasonCode { get; init; }
}
