using HboKommer.Shared.Contracts;

namespace HboKommer.Shared.Policy;

public interface IEventPolicyEvaluator
{
    PolicyDecision Evaluate(VisitStartedEventV1 evt, DateTimeOffset receivedAtUtc);
}
