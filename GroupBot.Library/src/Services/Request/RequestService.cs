using GroupBot.Library.Models;

namespace GroupBot.Library.Services.Request;

public class RequestService : IRequestService
{
    private readonly List<PendingRequest> _pendingRequests = [];

    public void Add(PendingRequest request)
    {
        if (_pendingRequests.Contains(request))
        {
            return;
        }

        _pendingRequests.Add(request);
    }

    public void Remove(PendingRequest request)
    {
        _pendingRequests.Remove(request);
    }

    public PendingRequest? GetRequest(long userId)
    {
        return _pendingRequests.FirstOrDefault(request => request.TargetUserTelegramId == userId);
    }
}