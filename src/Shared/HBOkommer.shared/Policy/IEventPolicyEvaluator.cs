using HBOkommer.Shared.Contracts;

namespace HBOkommer.Shared.Policy;

public interface IEventPolicyEvaluator
{
    PolicyDecision Evaluate(VisitStartedEventV1 evt, DateTimeOffset receivedAtUtc);
}
