using System.Data.Entity;
using System.Text.Json;
using GroupBot.Library.Models;
using Microsoft.Extensions.Configuration;

namespace GroupBot.Library.Services.Database;

public class DatabaseService : IDatabaseService, IDisposable
{
    private readonly BotDbContext _dbContext;
    private readonly SemaphoreSlim _listsSemaphore;
    private List<Participant> _admins;
    private List<ChatList> _lists;

    public DatabaseService(IConfiguration config)
    {
        var dbPath = config.GetSection("Database")["Path"];

        if (string.IsNullOrEmpty(dbPath))
        {
            throw new ArgumentException("Database path is missing in the configuration file");
        }

        _dbContext = new BotDbContext(dbPath);
        _admins = [];
        _lists = [];
        _listsSemaphore = new SemaphoreSlim(1, 1);
    }

    public void Initialize()
    {
        _dbContext.EnsureDatabaseCreated();

        const string jsonFilePath = "participants.json";

        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"Participants file is not found: {jsonFilePath}");

        var json = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var participants = JsonSerializer.Deserialize<List<Participant>>(json, options);

        if (participants == null)
            throw new ArgumentException($"There are no participants in JSON file: {jsonFilePath}");

        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            foreach (var participant in participants)
            {
                var user = new User
                {
                    TelegramId = participant.Id,
                    FullName = participant.Name,
                    CreatedAt = DateTime.UtcNow
                };

                if (!_dbContext.Users.Any(u => u.TelegramId == participant.Id))
                    _dbContext.Users.Add(user);
            }

            _dbContext.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        _dbContext.SaveChanges();
        Console.WriteLine("Database initialized");
    }

    public async Task<List<ChatList>> GetAllLists()
    {
        try
        {
            await _listsSemaphore.WaitAsync();

            _lists = await _dbContext.Lists
                .Include(l => l.Members)
                .ToListAsync();

            return _lists;
        }
        finally
        {
            _listsSemaphore.Release();
        }
    }


    public async Task<List<Participant>> GetAllListMembers(long listId)
    {
        return await _dbContext.ListMembers
            .Where(lm => lm.ListId == listId)
            .Include(lm => lm.User)
            .OrderBy(lm => lm.Position)
            .Select(lm => new Participant
            {
                Id = lm.User.TelegramId,
                Name = lm.User.FullName,
                Position = lm.Position
            })
            .ToListAsync();
    }


    public async Task<long> GetUserIdByTelegramId(long telegramId)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId);

        if (user == null)
        {
            throw new InvalidOperationException($"User with Telegram ID {telegramId} does not exist.");
        }

        return user.Id;
    }

    public async Task<long> GetUserIdByFullName(string fullName)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.FullName == fullName);

        if (user == null)
        {
            throw new InvalidOperationException($"User with full name {fullName} does not exist.");
        }

        return user.Id;
    }

    public async Task<long> CreateListAndShuffle(string listName)
    {
        using var transaction = _dbContext.Database.BeginTransaction();

        try
        {
            var chatList = new ChatList
            {
                Name = listName.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Lists.Add(chatList);
            await _dbContext.SaveChangesAsync();

            var users = await _dbContext.Users.ToListAsync();

            if (!users.Any())
                throw new InvalidOperationException("No users found to add to the list.");

            var random = new Random();
            var position = 1;

            foreach (var user in users.OrderBy(u => random.Next()))
            {
                var listMember = new ListMember
                {
                    ListId = chatList.Id,
                    UserId = user.Id,
                    Position = position++,
                    InsertedAt = DateTime.UtcNow,
                    List = chatList,
                    User = user
                };
                _dbContext.ListMembers.Add(listMember);
            }

            await _dbContext.SaveChangesAsync();
            transaction.Commit();

            _lists = await GetAllLists();
            return chatList.Id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task MoveUserToEndOfList(long listId, long userId)
    {
        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.TelegramId == userId);

            if (user == null)
                throw new InvalidOperationException($"User with Telegram ID {userId} does not exist.");

            var list = await _dbContext.Lists.FindAsync(listId);
            if (list == null)
                throw new InvalidOperationException($"List with ID {listId} does not exist.");

            var maxPosition = await _dbContext.ListMembers
                .Where(lm => lm.ListId == listId)
                .MaxAsync(lm => (int?)lm.Position) ?? 0;

            var existingMember = await _dbContext.ListMembers
                .FirstOrDefaultAsync(lm => lm.ListId == listId && lm.UserId == user.Id);

            if (existingMember != null)
            {
                existingMember.Position = maxPosition + 1;
            }
            else
            {
                var newMember = new ListMember
                {
                    ListId = listId,
                    UserId = user.Id,
                    Position = maxPosition + 1,
                    InsertedAt = DateTime.UtcNow,
                    List = list,
                    User = user
                };
                _dbContext.ListMembers.Add(newMember);
            }

            await _dbContext.SaveChangesAsync();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task SwapParticipantsInList(long listId, long userId, long targetUsedId)
    {
        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            var userMember = await _dbContext.ListMembers
                .FirstOrDefaultAsync(lm => lm.ListId == listId && lm.UserId == userId);
            var targetMember = await _dbContext.ListMembers
                .FirstOrDefaultAsync(lm => lm.ListId == listId && lm.UserId == targetUsedId);

            if (userMember == null || targetMember == null)
                throw new InvalidOperationException("One or both users not found in the list.");

            (userMember.Position, targetMember.Position) = (targetMember.Position, userMember.Position);

            await _dbContext.SaveChangesAsync();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<List<Participant>> GetAllAdmins()
    {
        if (_admins.Count > 0) return _admins;

        _admins = await _dbContext.Admins
            .Include(a => a.User)
            .Select(a => new Participant
            {
                Id = a.User.TelegramId,
                Name = a.User.FullName
            })
            .ToListAsync();

        return _admins;
    }

    public async Task RemoveList(long listId)
    {
        try
        {
            await _listsSemaphore.WaitAsync();

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var members = await _dbContext.ListMembers
                    .Where(lm => lm.ListId == listId)
                    .ToListAsync();

                if (members.Any())
                {
                    _dbContext.ListMembers.RemoveRange(members);
                    await _dbContext.SaveChangesAsync();
                }

                var list = await _dbContext.Lists
                    .FirstOrDefaultAsync(l => l.Id == listId);

                if (list != null)
                {
                    _dbContext.Lists.Remove(list);
                    await _dbContext.SaveChangesAsync();
                }

                transaction.Commit();

                _lists.RemoveAll(l => l.Id == listId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}, while removing list {listId}");
                transaction.Rollback();
                throw;
            }
        }
        finally
        {
            _listsSemaphore.Release();
        }
    }

    public async Task Sift(long listId, string userName)
    {
        try
        {
            await _listsSemaphore.WaitAsync();

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var members = await _dbContext.ListMembers
                    .Include(lm => lm.User)
                    .Where(lm => lm.ListId == listId)
                    .OrderBy(lm => lm.Position)
                    .ToListAsync();

                if (!members.Any())
                {
                    throw new InvalidOperationException("Список пуст");
                }

                var targetMember = members.FirstOrDefault(m => m.User.FullName == userName);

                if (targetMember == null)
                {
                    throw new InvalidOperationException($"Участник {userName} не найден в списке");
                }

                var beforeTarget = members
                    .Where(m => m.Position < targetMember.Position)
                    .Reverse()
                    .ToList();

                var afterTarget = members
                    .Where(m => m.Position > targetMember.Position)
                    .ToList();

                var position = 1;

                foreach (var member in afterTarget)
                {
                    member.Position = position++;
                }

                targetMember.Position = position++;

                foreach (var member in beforeTarget)
                {
                    member.Position = position++;
                }

                await _dbContext.SaveChangesAsync();
                transaction.Commit();

                var list = _lists.FirstOrDefault(l => l.Id == listId);

                if (list != null)
                {
                    list.Members = members.OrderBy(m => m.Position).ToList();
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        finally
        {
            _listsSemaphore.Release();
        }
    }

    public async Task AddAdmin(long userId)
    {
        try
        {
            await _listsSemaphore.WaitAsync();

            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            bool adminExists = await _dbContext.Admins.AnyAsync(a => a.UserId == userId);

            if (adminExists)
            {
                return;
            }

            var admin = new Admin
            {
                UserId = userId,
                User = user
            };

            _dbContext.Admins.Add(admin);
            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _listsSemaphore.Release();
        }
    }


    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
