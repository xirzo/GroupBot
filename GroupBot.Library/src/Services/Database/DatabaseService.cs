﻿using System.Data.Entity;
using System.Text.Json;
using GroupBot.Library.Models;
using Microsoft.Extensions.Configuration;

namespace GroupBot.Library.Services.Database;

public class DatabaseService : IDatabaseService, IDisposable
{
    private readonly IConfiguration _config;
    private readonly BotDbContext _dbContext;
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
        _config = config;
    }

    public void InitializeDatabase()
    {
        const string jsonFilePath = "participants.json";

        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"Participants file is not found: {jsonFilePath}");

        var json = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var participants = JsonSerializer.Deserialize<List<Participant>>(json, options);

        if (participants == null)
            throw new ArgumentException($"There are no participants in JSON file: {jsonFilePath}");

        foreach (var participant in participants)
        {
            var user = new User
            {
                TelegramId = participant.Id,
                FullName = participant.Name,
                CreatedAt = DateTime.UtcNow
            };

            if (!_dbContext.Users.Any(u => u.TelegramId == participant.Id)) _dbContext.Users.Add(user);
        }

        _dbContext.SaveChanges();
    }

    public async Task<List<ChatList>> GetAllLists()
    {
        if (_lists.Count > 0)
        {
            return _lists;
        }

        _lists = await _dbContext.Lists
            .ToListAsync();

        return _lists;
    }

    public async Task<List<Participant>> GetAllParticipantsInList(long id)
    {
        return await _dbContext.ListMembers
            .Where(lm => lm.ListId == id)
            .Include(lm => lm.User)
            .OrderBy(lm => lm.Position)
            .Select(lm => new Participant
            {
                Id = lm.User.TelegramId,
                Name = lm.User.FullName ?? "Unknown User",
                Position = lm.Position
            })
            .ToListAsync();
    }


    public async Task<long> GetParticipantIdByTelegramId(long id)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.TelegramId == id);

        if (user == null)
            throw new InvalidOperationException($"User with Telegram ID {id} does not exist.");

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

    public async Task SwapParticipantsInList(long id, long userDbId, long targetDbId)
    {
        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            var userMember = await _dbContext.ListMembers
                .FirstOrDefaultAsync(lm => lm.ListId == id && lm.UserId == userDbId);
            var targetMember = await _dbContext.ListMembers
                .FirstOrDefaultAsync(lm => lm.ListId == id && lm.UserId == targetDbId);

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
        using var transaction = _dbContext.Database.BeginTransaction();

        try
        {
            var members = await _dbContext.ListMembers
                .Where(lm => lm.ListId == listId)
                .ToListAsync();

            _dbContext.ListMembers.RemoveRange(members);

            var list = await _dbContext.Lists.FindAsync(listId);
            if (list != null) _dbContext.Lists.Remove(list);

            await _dbContext.SaveChangesAsync();
            transaction.Commit();

            _lists = await GetAllLists();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}