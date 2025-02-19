using GroupBot.Library.Lists;
using GroupBot.Library.Models;

namespace GroupBot.Library.Services.Database;

public interface IDatabaseService
{
    void InitializeDatabase();

    Task<List<ChatList>> GetAllLists();

    Task<long> CreateListAndShuffle(string listName);

    Task<List<Participant>> GetAllParticipantsInList(long id);

    Task<long> GetParticipantIdByTelegramId(long id);

    Task MoveUserToEndOfList(long listId, long userId);

    Task SwapParticipantsInList(long id, long userDbId, long targetDbId);
    Task<List<Participant>> GetAllAdmins();
    Task RemoveList(long listId);
}