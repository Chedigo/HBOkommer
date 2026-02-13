using HBOkommer.Shared.Contracts;

namespace HBOkommer.Shared.Policy;

public sealed class PilotEventPolicyEvaluator : IEventPolicyEvaluator
{
    private static readonly TimeSpan MaxAge = TimeSpan.FromHours(2);

    public PolicyDecision Evaluate(VisitStartedEventV1 evt, DateTimeOffset receivedAtUtc)
    {
        var age = receivedAtUtc - evt.OccurredAtUtc;
        if (age > MaxAge)
            return new PolicyDecision { Eligible = false, ReasonCode = "EVENT_TOO_OLD" };

        // PILOT: samtykke antas OK (flyttes til IConsentChecker i neste commit)
        return new PolicyDecision { Eligible = true, ReasonCode = "OK" };
    }
}
