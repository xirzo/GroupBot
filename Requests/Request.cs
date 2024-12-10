namespace GroupBot.Requests;

public static class PendingRequestsContainer
{
    private static readonly List<PendingRequest> _pendingRequests = new();

    public static void AddRequest(PendingRequest request)
    {
        if (_pendingRequests.Contains(request)) return;

        _pendingRequests.Add(request);
    }

    public static void RemoveRequest(PendingRequest request)
    {
        _pendingRequests.Remove(request);
    }

    public static PendingRequest? GetRequest(long userId)
    {
        return _pendingRequests.FirstOrDefault(r => r.UserId == userId);
    }
}

public record struct PendingRequest
{
    public long UserId { get; set; }
    public long TargetUserId { get; set; }
    public string ListName { get; set; }
}