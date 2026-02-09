using HboKommer.Shared.Contracts;
using HboKommer.Shared.Policy;

namespace HboKommer.Api.Policy;

public sealed class PilotEventPolicyEvaluator : IEventPolicyEvaluator
{
    // Gyldighetsvindu (Trinn 3 / NFR: avvis for sent mottatte hendelser)
    // Vi setter et konservativt vindu i pilot. Kan flyttes til konfig i Trinn 9.
    private static readonly TimeSpan MaxAge = TimeSpan.FromHours(2);

    // Pilot: samtykke-modell finnes ikke ennå (kommer via konfig/policy senere).
    // Inntil Trinn 9: vi "tillater" varsling for pilot for ikke å blokkere hele flyten,
    // men vi returnerer fortsatt beslutningen eksplisitt.
    private const bool PilotConsentGranted = true;

    public PolicyDecision Evaluate(VisitStartedEventV1 evt, DateTimeOffset receivedAtUtc)
    {
        if (!PilotConsentGranted)
            return new PolicyDecision { Eligible = false, ReasonCode = "CONSENT_NOT_GRANTED" };

        var age = receivedAtUtc - evt.OccurredAtUtc;
        if (age > MaxAge)
            return new PolicyDecision { Eligible = false, ReasonCode = "EVENT_TOO_OLD" };

        // Innholdsbegrensning (Trinn 3): vi sender ikke SMS nå,
        // men vi legger grunnlaget: ingen fritekst/journal/helseopplysninger i event.
        // (Kontrakten din har allerede ingen tekstfelt – bra.)
        return new PolicyDecision { Eligible = true, ReasonCode = "OK" };
    }
}
