using GroupBot.Library.Models;

namespace GroupBot.Library.Services.Database;

public interface IDatabaseService
{
    void Initialize();
    Task<List<ChatList>> GetAllLists();
    Task<long> CreateListAndShuffle(string listName);
    Task<List<Participant>> GetAllListMembers(long id);
    Task<long> GetUserIdByTelegramId(long id);
    Task MoveUserToEndOfList(long listId, long userId);
    Task SwapParticipantsInList(long id, long userDbId, long targetDbId);
    Task<List<Participant>> GetAllAdmins();
    Task RemoveList(long listId);
    Task Sift(long listId, string userName);
}
