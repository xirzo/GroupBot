using GroupBot.Lists;
using GroupBot.Parser;

namespace GroupBot.Services.Database;

public interface IDatabaseService
{
  void InitializeDatabase();

  Task<List<ChatList>> GetAllLists();

  long CreateListAndShuffle(string listName);

  Task<List<Participant>> GetAllParticipantsInList(long id);

  Task<long> GetParticipantIdByTelegramId(long id);

  Task MoveUserToEndOfList(long listId, long userId);

  Task SwapParticipantsInList(long id, long userDbId, long targetDbId);
}
