namespace GroupBot.Requests;

public class RequestsContainer
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
        foreach (var request in _pendingRequests.Where(request => userId == request.TargetUserTelegramId))
        {
            return request;
        }

        return null;
    }
}
