using GroupBot.Library.Models;

namespace GroupBot.Library.Services.Request;

public interface IRequestService
{
    void Add(PendingRequest request);
    void Remove(PendingRequest request);
    PendingRequest? GetRequest(long userId);
}