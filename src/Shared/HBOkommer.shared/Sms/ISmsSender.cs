namespace HBOkommer.Shared.Sms;

public interface ISmsSender
{
    Task<SmsSendResult> SendAsync(SmsSendRequest request, CancellationToken ct = default);
}
