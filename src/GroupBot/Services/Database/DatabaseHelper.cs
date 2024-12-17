using System.Data;
using System.Data.SQLite;
using GroupBot.Lists;
using GroupBot.Parser;

namespace GroupBot.Services.Database
{
  public class DatabaseHelper
  {
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHelper"/> class with the specified database path.
    /// </summary>
    /// <param name="dbPath">The file path to the SQLite database.</param>
    public DatabaseHelper(string dbPath)
    {
      _connectionString = $"Data Source={dbPath};Version=3;";

      if (!File.Exists(dbPath)) SQLiteConnection.CreateFile(dbPath);

      Initialize().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes the database by creating necessary tables and ensuring the presence of required columns.
    /// </summary>
    private async Task Initialize()
    {
      using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      var createUsersTableQuery = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    telegram_id INTEGER UNIQUE NOT NULL,
                    full_name TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
            ";
      await using (var command = new SQLiteCommand(createUsersTableQuery, connection))
      {
        await command.ExecuteNonQueryAsync();
      }

      var createListsTableQuery = @"
                CREATE TABLE IF NOT EXISTS lists (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    list_name TEXT UNIQUE NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
            ";
      await using (var command = new SQLiteCommand(createListsTableQuery, connection))
      {
        await command.ExecuteNonQueryAsync();
      }

      var createListMembersTableQuery = @"
                CREATE TABLE IF NOT EXISTS list_members (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    list_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    position INTEGER,
                    inserted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (list_id) REFERENCES lists(id),
                    FOREIGN KEY (user_id) REFERENCES users(id),
                    UNIQUE(list_id, user_id)
                );
            ";
      await using (var command = new SQLiteCommand(createListMembersTableQuery, connection))
      {
        await command.ExecuteNonQueryAsync();
      }

      var positionExists = false;
      var pragmaQuery = "PRAGMA table_info(list_members);";
      await using (var pragmaCmd = new SQLiteCommand(pragmaQuery, connection))
      await using (var reader = await pragmaCmd.ExecuteReaderAsync())
      {
        while (await reader.ReadAsync())
        {
          var columnName = reader.GetString(1);
          if (string.Equals(columnName, "position", StringComparison.OrdinalIgnoreCase))
          {
            positionExists = true;
            break;
          }
        }
      }

      if (!positionExists)
      {
        var alterTableQuery = "ALTER TABLE list_members ADD COLUMN position INTEGER;";
        using (var alterCmd = new SQLiteCommand(alterTableQuery, connection))
        {
          await alterCmd.ExecuteNonQueryAsync();
        }

        var updatePositionsQuery = @"
                    UPDATE list_members
                    SET position = (
                        SELECT COUNT(*) 
                        FROM list_members AS lm2 
                        WHERE lm2.list_id = list_members.list_id AND lm2.id <= list_members.id
                    );
                ";
        await using (var updateCmd = new SQLiteCommand(updatePositionsQuery, connection))
        {
          await updateCmd.ExecuteNonQueryAsync();
        }
      }

      Console.WriteLine("Database initialization complete.");
    }

    /// <summary>
    /// Inserts a list of participants into the users table, ignoring duplicates.
    /// </summary>
    /// <param name="participants">The list of participants to insert.</param>
    public void InsertParticipants(List<Participant> participants)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      foreach (var participant in participants)
      {
        var insertQuery =
            @"INSERT OR IGNORE INTO users (telegram_id, full_name) VALUES (@telegram_id, @full_name);";

        using var command = new SQLiteCommand(insertQuery, connection);
        command.Parameters.AddWithValue("@telegram_id", participant.Id);
        command.Parameters.AddWithValue("@full_name", participant.Name);
        command.ExecuteNonQuery();
      }
    }

    /// <summary>
    /// Executes a non-query SQL command asynchronously.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteQueryAsync(string query, params SQLiteParameter[]? parameters)
    {
      await using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      await using var command = new SQLiteCommand(query, connection);

      if (parameters != null) command.Parameters.AddRange(parameters);

      await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Creates a new user in the users table.
    /// </summary>
    /// <param name="telegramId">The Telegram ID of the user.</param>
    /// <param name="fullName">The full name of the user.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating success.</returns>
    public async Task<bool> CreateUser(long telegramId, string fullName)
    {
      await using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      const string insertQuery =
          @"INSERT OR IGNORE INTO users (telegram_id, full_name) VALUES (@telegram_id, @full_name);";

      await using var command = new SQLiteCommand(insertQuery, connection);
      command.Parameters.AddWithValue("@telegram_id", telegramId);
      command.Parameters.AddWithValue("@full_name", fullName);
      await command.ExecuteNonQueryAsync();
      return true;
    }

    /// <summary>
    /// Checks asynchronously if a user exists in the users table based on the Telegram ID.
    /// </summary>
    /// <param name="telegramId">The Telegram ID of the user.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating existence.</returns>
    public async Task<bool> DoesUserExist(long telegramId)
    {
      await using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      await using var command =
          new SQLiteCommand("SELECT COUNT(1) FROM users WHERE telegram_id = @telegram_id", connection);
      command.Parameters.AddWithValue("@telegram_id", telegramId);

      var count = Convert.ToInt32(await command.ExecuteScalarAsync());
      return count > 0;
    }

    private long CreateList(string listName, SQLiteConnection connection, SQLiteTransaction transaction)
    {
      const string insertListQuery = @"
                INSERT INTO lists (list_name)
                VALUES (@listName);
            ";

      using (var insertListCmd = new SQLiteCommand(insertListQuery, connection, transaction))
      {
        insertListCmd.Parameters.AddWithValue("@listName", listName.Trim());
        insertListCmd.ExecuteNonQuery();

        using (var getIdCmd = new SQLiteCommand("SELECT last_insert_rowid();", connection, transaction))
        {
          var result = getIdCmd.ExecuteScalar();
          if (result == null || !long.TryParse(result.ToString(), out var listId))
            throw new InvalidOperationException("Failed to retrieve the new list ID.");
          return listId;
        }
      }
    }

    /// <summary>
    /// Creates a new list with the specified name and shuffles all users into it by assigning shuffled positions.
    /// </summary>
    /// <param name="listName">The name of the new list.</param>
    /// <returns>The ID of the newly created list.</returns>
    public long CreateListAndShuffle(string listName)
    {
      if (string.IsNullOrWhiteSpace(listName))
        throw new ArgumentException("List name cannot be null or empty.", nameof(listName));

      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      using var transaction = connection.BeginTransaction();

      try
      {
        var listId = CreateList(listName, connection, transaction);

        var users = new List<int>();
        const string selectUsersQuery = "SELECT id FROM users;";

        using (var selectUsersCmd = new SQLiteCommand(selectUsersQuery, connection, transaction))
        using (var reader = selectUsersCmd.ExecuteReader())
        {
          while (reader.Read())
          {
            if (!reader.IsDBNull(0))
            {
              var userId = reader.GetInt32(0);
              users.Add(userId);
            }
          }
        }

        if (users.Count == 0)
        {
          throw new InvalidOperationException("No users found in the 'users' table to add to the list.");
        }

        var random = new Random();
        var shuffledUserIds = users.OrderBy(u => random.Next()).ToList();

        const string insertMemberQuery = @"
                    INSERT INTO list_members (list_id, user_id, position)
                    VALUES (@listId, @userId, @position);
                ";

        using (var insertMemberCmd = new SQLiteCommand(insertMemberQuery, connection, transaction))
        {
          var listIdParam = insertMemberCmd.Parameters.Add("@listId", DbType.Int64);
          var userIdParam = insertMemberCmd.Parameters.Add("@userId", DbType.Int32);
          var positionParam = insertMemberCmd.Parameters.Add("@position", DbType.Int32);

          listIdParam.Value = listId;
          int position = 1;

          foreach (var userId in shuffledUserIds)
          {
            userIdParam.Value = userId;
            positionParam.Value = position++;
            insertMemberCmd.ExecuteNonQuery();
          }
        }

        transaction.Commit();

        Console.WriteLine($"List '{listName}' created with ID {listId} and all users shuffled into it.");
        return listId;
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        Console.Error.WriteLine($"Error in CreateListAndShuffle: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Retrieves all users from the users table.
    /// </summary>
    /// <returns>A list of all participants.</returns>
    public List<Participant> GetAllUsers()
    {
      var participants = new List<Participant>();

      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      var selectQuery = "SELECT telegram_id, full_name FROM users;";
      using var command = new SQLiteCommand(selectQuery, connection);

      using var reader = command.ExecuteReader();

      while (reader.Read())
      {
        var telegramId = reader.GetInt64(0);
        var fullName = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty;

        var participant = new Participant
        {
          Id = telegramId,
          Name = fullName
        };

        participants.Add(participant);
      }

      return participants;
    }

    /// <summary>
    /// Retrieves all chat lists from the lists table.
    /// </summary>
    /// <returns>A list of all chat lists.</returns>
    public async Task<List<ChatList>> GetAllLists()
    {
      var result = new List<ChatList>();

      await using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      var query = "SELECT id, list_name, created_at FROM lists;";
      await using var command = new SQLiteCommand(query, connection);
      await using var reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        var chatList = new ChatList(reader.GetString(1), reader.GetInt64(0));
        result.Add(chatList);
      }

      return result;
    }

    /// <summary>
    /// Asynchronously retrieves the user ID based on the provided Telegram ID.
    /// </summary>
    /// <param name="telegramId">The Telegram ID of the user.</param>
    /// <returns>A task representing the asynchronous operation, with the user's ID as the result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the user does not exist.</exception>
    public async Task<long> GetParticipantIdByTelegramId(long telegramId)
    {
      await using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      var query = "SELECT id FROM users WHERE telegram_id = @telegramId;";
      await using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@telegramId", telegramId);

      var result = await command.ExecuteScalarAsync();
      if (result != null && long.TryParse(result.ToString(), out var userId)) return userId;

      throw new InvalidOperationException($"User with Telegram ID {telegramId} does not exist.");
    }

    /// <summary>
    /// Attempts to add a user to a specific chat list.
    /// </summary>
    /// <param name="listId">The ID of the chat list.</param>
    /// <param name="telegramId">The Telegram ID of the user to add.</param>
    /// <returns>True if the operation was successful; otherwise, false.</returns>
    public async Task<bool> TryAddUserToList(long listId, long telegramId)
    {
      if (listId <= 0)
        throw new ArgumentException("List ID must be a positive number.", nameof(listId));

      await using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      var checkListQuery = "SELECT COUNT(1) FROM lists WHERE id = @list_id;";
      await using (var checkListCmd = new SQLiteCommand(checkListQuery, connection))
      {
        checkListCmd.Parameters.AddWithValue("@list_id", listId);
        var listExists = Convert.ToInt32(await checkListCmd.ExecuteScalarAsync()) > 0;
        if (!listExists)
          throw new InvalidOperationException($"List with ID {listId} does not exist.");
      }

      var checkUserQuery = "SELECT COUNT(1) FROM users WHERE telegram_id = @telegramId;";
      await using (var checkUserCmd = new SQLiteCommand(checkUserQuery, connection))
      {
        checkUserCmd.Parameters.AddWithValue("@telegramId", telegramId);
        var userExists = Convert.ToInt32(await checkUserCmd.ExecuteScalarAsync()) > 0;
        if (!userExists)
          throw new InvalidOperationException($"User with Telegram ID {telegramId} does not exist.");
      }

      var userId = await GetParticipantIdByTelegramId(telegramId);

      var checkMembershipQuery = @"
                SELECT COUNT(1)
                FROM list_members
                WHERE list_id = @list_id AND user_id = @user_id;
            ";
      await using (var checkMembershipCmd = new SQLiteCommand(checkMembershipQuery, connection))
      {
        checkMembershipCmd.Parameters.AddWithValue("@list_id", listId);
        checkMembershipCmd.Parameters.AddWithValue("@user_id", userId);
        var isMember = Convert.ToInt32(await checkMembershipCmd.ExecuteScalarAsync()) > 0;
        if (isMember)
          return false;
      }

      var getMaxPositionQuery = @"
                SELECT IFNULL(MAX(position), 0)
                FROM list_members
                WHERE list_id = @list_id;
            ";
      var newPosition = 1;
      await using (var maxPosCmd = new SQLiteCommand(getMaxPositionQuery, connection))
      {
        maxPosCmd.Parameters.AddWithValue("@list_id", listId);
        var maxPosResult = await maxPosCmd.ExecuteScalarAsync();
        if (maxPosResult != null && int.TryParse(maxPosResult.ToString(), out var maxPosition))
          newPosition = maxPosition + 1;
      }

      var insertQuery = @"
                INSERT INTO list_members (list_id, user_id, position, inserted_at)
                VALUES (@list_id, @user_id, @position, @inserted_at);
            ";

      await using var command = new SQLiteCommand(insertQuery, connection);
      command.Parameters.AddWithValue("@list_id", listId);
      command.Parameters.AddWithValue("@user_id", userId);
      command.Parameters.AddWithValue("@position", newPosition);
      command.Parameters.AddWithValue("@inserted_at", DateTime.UtcNow);

      try
      {
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
      }
      catch (SQLiteException ex)
      {
        await Console.Error.WriteLineAsync($"SQLite error: {ex.Message}");
        return false;
      }
    }

    /// <summary>
    /// Swaps the positions of two users within a specified list.
    /// </summary>
    /// <param name="listId">The ID of the list.</param>
    /// <param name="userDbId">The database ID of the first user.</param>
    /// <param name="targetDbId">The database ID of the second user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if either user is not found in the list.</exception>
    public async Task SwapParticipantsInList(long listId, long userDbId, long targetDbId)
    {
      await using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      await using var transaction = connection.BeginTransaction();

      try
      {
        var userPosition = await GetUserPositionAsync(connection, transaction, listId, userDbId);
        if (userPosition == null)
          throw new InvalidOperationException(
              $"User with ID {userDbId} not found in list with ID {listId}.");

        var targetPosition = await GetUserPositionAsync(connection, transaction, listId, targetDbId);
        if (targetPosition == null)
          throw new InvalidOperationException(
              $"User with ID {targetDbId} not found in list with ID {listId}.");

        await UpdateUserPositionAsync(connection, transaction, listId, userDbId, targetPosition.Value);
        await UpdateUserPositionAsync(connection, transaction, listId, targetDbId, userPosition.Value);

        transaction.Commit();
      }
      catch
      {
        transaction.Rollback();
        throw;
      }
      finally
      {
        await connection.CloseAsync();
      }
    }

    private async Task<int?> GetUserPositionAsync(SQLiteConnection connection, SQLiteTransaction transaction,
        long listId, long userId)
    {
      var query = @"
                SELECT position
                FROM list_members
                WHERE list_id = @listId AND user_id = @userId;
            ";

      await using var command = new SQLiteCommand(query, connection, transaction);
      command.Parameters.AddWithValue("@listId", listId);
      command.Parameters.AddWithValue("@userId", userId);

      var result = await command.ExecuteScalarAsync();
      if (result != null && int.TryParse(result.ToString(), out var position))
        return position;
      return null;
    }

    private async Task UpdateUserPositionAsync(SQLiteConnection connection, SQLiteTransaction transaction,
        long listId, long userId, int newPosition)
    {
      var updateQuery = @"
                UPDATE list_members
                SET position = @newPosition
                WHERE list_id = @listId AND user_id = @userId;
            ";

      await using var updateCommand = new SQLiteCommand(updateQuery, connection, transaction);
      updateCommand.Parameters.AddWithValue("@newPosition", newPosition);
      updateCommand.Parameters.AddWithValue("@listId", listId);
      updateCommand.Parameters.AddWithValue("@userId", userId);

      await updateCommand.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Retrieves all users in the specified list, including their positions, sorted in ascending order of position.
    /// </summary>
    /// <param name="listId">The ID of the list.</param>
    /// <returns>A list of participants with their positions.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the list does not exist.</exception>
    public async Task<List<Participant>> GetAllUsersInList(long listId)
    {
      var participants = new List<Participant>();

      using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      const string checkListQuery = "SELECT COUNT(1) FROM lists WHERE id = @list_id;";
      using (var checkListCmd = new SQLiteCommand(checkListQuery, connection))
      {
        checkListCmd.Parameters.AddWithValue("@list_id", listId);
        var listExists = Convert.ToInt32(await checkListCmd.ExecuteScalarAsync()) > 0;
        if (!listExists)
          throw new InvalidOperationException($"List with ID {listId} does not exist.");
      }

      const string selectUsersQuery = @"
                SELECT u.telegram_id, u.full_name, lm.position
                FROM list_members lm
                JOIN users u ON lm.user_id = u.id
                WHERE lm.list_id = @list_id
                ORDER BY lm.position ASC;
            ";

      using var command = new SQLiteCommand(selectUsersQuery, connection);
      command.Parameters.AddWithValue("@list_id", listId);

      using var reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        var telegramId = reader.GetInt64(0);
        var fullName = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty;
        var position = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

        var participant = new Participant
        {
          Id = telegramId,
          Name = fullName,
          Position = position
        };

        participants.Add(participant);
      }

      return participants;
    }

    /// <summary>
    /// Moves a user to the end of the specified list. If the user is not in the list, they are added.
    /// </summary>
    /// <param name="listId">The ID of the list.</param>
    /// <param name="telegramId">The Telegram ID of the user.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating success.</returns>
    public async Task<bool> MoveUserToEndOfListAsync(long listId, long telegramId)
    {
      if (listId <= 0)
        throw new ArgumentException("List ID must be a positive number.", nameof(listId));

      await using var connection = new SQLiteConnection(_connectionString);
      await connection.OpenAsync();

      using var transaction = connection.BeginTransaction();

      try
      {
        var checkListQuery = "SELECT COUNT(1) FROM lists WHERE id = @list_id;";
        await using (var checkListCmd = new SQLiteCommand(checkListQuery, connection, transaction))
        {
          checkListCmd.Parameters.AddWithValue("@list_id", listId);
          var listExists = Convert.ToInt32(await checkListCmd.ExecuteScalarAsync()) > 0;
          if (!listExists)
            throw new InvalidOperationException($"List with ID {listId} does not exist.");
        }

        var checkUserQuery = "SELECT COUNT(1) FROM users WHERE telegram_id = @telegramId;";
        await using (var checkUserCmd = new SQLiteCommand(checkUserQuery, connection, transaction))
        {
          checkUserCmd.Parameters.AddWithValue("@telegramId", telegramId);
          var userExists = Convert.ToInt32(await checkUserCmd.ExecuteScalarAsync()) > 0;
          if (!userExists)
            throw new InvalidOperationException($"User with Telegram ID {telegramId} does not exist.");
        }

        var userId = await GetParticipantIdByTelegramId(telegramId);

        var getMaxPositionQuery = @"
                    SELECT IFNULL(MAX(position), 0)
                    FROM list_members
                    WHERE list_id = @list_id;
                ";
        var newPosition = 1;
        using (var maxPosCmd = new SQLiteCommand(getMaxPositionQuery, connection, transaction))
        {
          maxPosCmd.Parameters.AddWithValue("@list_id", listId);
          var maxPosResult = await maxPosCmd.ExecuteScalarAsync();
          if (maxPosResult != null && int.TryParse(maxPosResult.ToString(), out var maxPosition))
            newPosition = maxPosition + 1;
        }

        var checkMembershipQuery = @"
                    SELECT position
                    FROM list_members
                    WHERE list_id = @list_id AND user_id = @user_id;
                ";
        int? existingPosition = null;
        using (var checkMembershipCmd = new SQLiteCommand(checkMembershipQuery, connection, transaction))
        {
          checkMembershipCmd.Parameters.AddWithValue("@list_id", listId);
          checkMembershipCmd.Parameters.AddWithValue("@user_id", userId);
          var result = await checkMembershipCmd.ExecuteScalarAsync();
          if (result != null && int.TryParse(result.ToString(), out var pos)) existingPosition = pos;
        }

        if (existingPosition.HasValue)
        {
          var updatePositionQuery = @"
                        UPDATE list_members
                        SET position = @newPosition
                        WHERE list_id = @list_id AND user_id = @user_id;
                    ";
          using (var updateCmd = new SQLiteCommand(updatePositionQuery, connection, transaction))
          {
            updateCmd.Parameters.AddWithValue("@newPosition", newPosition);
            updateCmd.Parameters.AddWithValue("@list_id", listId);
            updateCmd.Parameters.AddWithValue("@user_id", userId);
            await updateCmd.ExecuteNonQueryAsync();
          }
        }
        else
        {
          var insertQuery = @"
                        INSERT INTO list_members (list_id, user_id, position, inserted_at)
                        VALUES (@list_id, @user_id, @position, @inserted_at);
                    ";

          await using var insertCmd = new SQLiteCommand(insertQuery, connection, transaction);
          insertCmd.Parameters.AddWithValue("@list_id", listId);
          insertCmd.Parameters.AddWithValue("@user_id", userId);
          insertCmd.Parameters.AddWithValue("@position", newPosition);
          insertCmd.Parameters.AddWithValue("@inserted_at", DateTime.UtcNow);

          try
          {
            var rowsAffected = await insertCmd.ExecuteNonQueryAsync();
            if (rowsAffected <= 0)
              return false;
          }
          catch (SQLiteException ex)
          {
            await Console.Error.WriteLineAsync($"SQLite error: {ex.Message}");
            return false;
          }
        }

        transaction.Commit();
        return true;
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        await Console.Error.WriteLineAsync($"Error in MoveUserToEndOfListAsync: {ex.Message}");
        throw;
      }
      finally
      {
        await connection.CloseAsync();
      }
    }
  }
}
