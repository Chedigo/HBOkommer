using HBOkommer.Shared.Sms;
using Microsoft.Extensions.Logging;

namespace HBOkommer.Api.Sms;

public sealed class PilotSmsSender : ISmsSender
{
    private readonly ILogger<PilotSmsSender> _logger;

    public PilotSmsSender(ILogger<PilotSmsSender> logger)
    {
        _logger = logger;
    }

    public Task<SmsSendResult> SendAsync(SmsSendRequest request, CancellationToken ct = default)
    {
        // Trinn 5: Adapter finnes. Trinn 7/8 avgjør når den faktisk brukes.
        // Safe-by-default: Vi “simulerer” sending og logger.

        _logger.LogInformation(
            "PILOT_SMS_SENDER_SIMULATED municipalityId={MunicipalityId} eventId={EventId} to={ToPhoneE164}",
            request.MunicipalityId, request.EventId, request.ToPhoneE164);

        return Task.FromResult(new SmsSendResult
        {
            Success = true,
            ProviderMessageId = "SIMULATED",
            ReasonCode = "OK"
        });
    }
}
