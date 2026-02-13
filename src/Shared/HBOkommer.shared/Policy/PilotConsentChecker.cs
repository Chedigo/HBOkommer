namespace HBOkommer.Shared.Policy;

public sealed class PilotConsentChecker : IConsentChecker
{
    public bool HasConsent(string municipalityId, string unitId, string subjectRef)
    {
        // PILOT: samtykke antas OK for å la flyten gå videre i testmiljø.
        // Senere: erstatt med faktisk samtykkeoppslag (via integrasjonslag/EPJ).
        return true;
    }
}
