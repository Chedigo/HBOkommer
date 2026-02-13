namespace HBOkommer.Shared.Policy;

public interface IConsentChecker
{
    bool HasConsent(string municipalityId, string unitId, string subjectRef);
}
