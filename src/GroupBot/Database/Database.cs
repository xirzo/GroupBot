using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GroupBot.Lists;
using GroupBot.Parser;

namespace GroupBot.Database
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            Initialize().GetAwaiter().GetResult();
        }

        private async Task Initialize()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            // Создание таблицы users
            var createUsersTableQuery = @"
CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    telegram_id INTEGER UNIQUE NOT NULL,
    full_name TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
";
            using (var command = new SQLiteCommand(createUsersTableQuery, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Создание таблицы lists
            var createListsTableQuery = @"
CREATE TABLE IF NOT EXISTS lists (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    list_name TEXT UNIQUE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
";
            using (var command = new SQLiteCommand(createListsTableQuery, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Создание таблицы list_members с добавлением столбца position
            var createListMembersTableQuery = @"
CREATE TABLE IF NOT EXISTS list_members (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    list_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    position INTEGER,
    inserted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (list_id) REFERENCES lists(id),
    FOREIGN KEY (user_id) REFERENCES users(id),
    UNIQUE(list_id, user_id) -- Предотвращает дублирование пользователей в списке
);
";
            using (var command = new SQLiteCommand(createListMembersTableQuery, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Проверка наличия столбца position и добавление его, если отсутствует
            bool positionExists = false;
            var pragmaQuery = "PRAGMA table_info(list_members);";
            using (var pragmaCmd = new SQLiteCommand(pragmaQuery, connection))
            using (var reader = await pragmaCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string columnName = reader.GetString(1);
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

                // Установка позиции для существующих записей
                var updatePositionsQuery = @"
UPDATE list_members
SET position = (
    SELECT COUNT(*) 
    FROM list_members AS lm2 
    WHERE lm2.list_id = list_members.list_id AND lm2.id <= list_members.id
);
";
                using (var updateCmd = new SQLiteCommand(updatePositionsQuery, connection))
                {
                    await updateCmd.ExecuteNonQueryAsync();
                }

                Console.WriteLine(
                    "Добавлен столбец 'position' в таблицу 'list_members' и установлены значения позиций.");
            }

            Console.WriteLine("Database initialization complete.");
        }

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

        public async Task ExecuteQueryAsync(string query, params SQLiteParameter[]? parameters)
        {
            await using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SQLiteCommand(query, connection);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            await command.ExecuteNonQueryAsync();
        }

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

        private long CreateList(string listName)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string insertListQuery = @"
                INSERT INTO lists (list_name)
                VALUES (@list_name);
            ";

            using (var command = new SQLiteCommand(insertListQuery, connection))
            {
                command.Parameters.AddWithValue("@list_name", listName);
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand("SELECT last_insert_rowid();", connection))
            {
                var listId = (long)command.ExecuteScalar();
                return listId;
            }
        }

        public long CreateListAndShuffle(string listName)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            long listId = CreateList(listName);

            var users = new List<(int userId, long telegramId, string fullName)>();
            var selectUsersQuery = "SELECT id, telegram_id, full_name FROM users;";

            using (var selectCommand = new SQLiteCommand(selectUsersQuery, connection))
            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var userId = reader.GetInt32(0);
                    var telegramId = reader.GetInt64(1);
                    var fullName = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    users.Add((userId, telegramId, fullName));
                }
            }

            var random = new Random();
            var shuffledUsers = users.OrderBy(u => random.Next()).ToList();

            var insertMemberQuery = @"
               INSERT INTO list_members (list_id, user_id, position)
               VALUES (@list_id, @user_id, @position);
           ";

            using (var insertCommand = new SQLiteCommand(insertMemberQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@list_id", listId);

                var userIdParam = insertCommand.Parameters.Add("@user_id", System.Data.DbType.Int32);
                var positionParam = insertCommand.Parameters.Add("@position", System.Data.DbType.Int32);

                int position = 1;
                foreach (var user in shuffledUsers)
                {
                    userIdParam.Value = user.userId;
                    positionParam.Value = position++;
                    insertCommand.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"All users inserted into list '{listName}' in random order.");
            return listId;
        }

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
                var telegramId = reader.GetInt64(0); // telegram_id is stored as a long/int64
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
                var chatList = new ChatList(reader.GetString(1), reader.GetInt64(0), this);
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
        public async Task<long> GetUserIdByTelegramIdAsync(long telegramId)
        {
            await using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT id FROM users WHERE telegram_id = @telegramId;";
            await using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);

            var result = await command.ExecuteScalarAsync();
            if (result != null && long.TryParse(result.ToString(), out long userId))
            {
                return userId;
            }

            throw new InvalidOperationException($"User with Telegram ID {telegramId} does not exist.");
        }

        /// <summary>
        /// Adds a user to a specific chat list.
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

            // Проверяем существование списка
            var checkListQuery = "SELECT COUNT(1) FROM lists WHERE id = @list_id;";
            await using (var checkListCmd = new SQLiteCommand(checkListQuery, connection))
            {
                checkListCmd.Parameters.AddWithValue("@list_id", listId);
                var listExists = Convert.ToInt32(await checkListCmd.ExecuteScalarAsync()) > 0;
                if (!listExists)
                    throw new InvalidOperationException($"List with ID {listId} does not exist.");
            }

            // Проверяем существование пользователя
            var checkUserQuery = "SELECT COUNT(1) FROM users WHERE telegram_id = @telegramId;";
            await using (var checkUserCmd = new SQLiteCommand(checkUserQuery, connection))
            {
                checkUserCmd.Parameters.AddWithValue("@telegramId", telegramId);
                var userExists = Convert.ToInt32(await checkUserCmd.ExecuteScalarAsync()) > 0;
                if (!userExists)
                    throw new InvalidOperationException($"User with Telegram ID {telegramId} does not exist.");
            }

            // Получаем user_id из telegram_id
            var userId = await GetUserIdByTelegramIdAsync(telegramId);

            // Проверяем, уже ли пользователь в списке
            var checkMembershipQuery = @"
                SELECT COUNT(1)
                FROM list_members
                WHERE list_id = @list_id AND user_id = @user_id;
            ";
            using (var checkMembershipCmd = new SQLiteCommand(checkMembershipQuery, connection))
            {
                checkMembershipCmd.Parameters.AddWithValue("@list_id", listId);
                checkMembershipCmd.Parameters.AddWithValue("@user_id", userId);
                var isMember = Convert.ToInt32(await checkMembershipCmd.ExecuteScalarAsync()) > 0;
                if (isMember)
                {
                    // Пользователь уже в списке
                    return false;
                }
            }

            // Получаем максимальную позицию в списке
            var getMaxPositionQuery = @"
                SELECT IFNULL(MAX(position), 0)
                FROM list_members
                WHERE list_id = @list_id;
            ";
            int newPosition = 1;
            using (var maxPosCmd = new SQLiteCommand(getMaxPositionQuery, connection))
            {
                maxPosCmd.Parameters.AddWithValue("@list_id", listId);
                var maxPosResult = await maxPosCmd.ExecuteScalarAsync();
                if (maxPosResult != null && int.TryParse(maxPosResult.ToString(), out int maxPosition))
                {
                    newPosition = maxPosition + 1;
                }
            }

            // Вставляем пользователя в список с назначенной позицией
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
                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (SQLiteException ex)
            {
                await Console.Error.WriteLineAsync($"SQLite error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Обменивает позиции двух пользователей в определённом списке.
        /// </summary>
        /// <param name="listId">Идентификатор списка.</param>
        /// <param name="userDbId">Идентификатор первого пользователя.</param>
        /// <param name="targetDbId">Идентификатор второго пользователя.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        /// <exception cref="InvalidOperationException">Бросается, если один из пользователей не найден в списке.</exception>
        public async Task SwapUsersInListAsync(long listId, long userDbId, long targetDbId)
        {
            await using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Получаем позиции обоих пользователей
                var userPosition = await GetUserPositionAsync(connection, transaction, listId, userDbId);
                if (userPosition == null)
                    throw new InvalidOperationException(
                        $"Пользователь с ID {userDbId} не найден в списке с ID {listId}.");

                var targetPosition = await GetUserPositionAsync(connection, transaction, listId, targetDbId);
                if (targetPosition == null)
                    throw new InvalidOperationException(
                        $"Пользователь с ID {targetDbId} не найден в списке с ID {listId}.");

                // Обмениваем позиции
                await UpdateUserPositionAsync(connection, transaction, listId, userDbId, targetPosition.Value);
                await UpdateUserPositionAsync(connection, transaction, listId, targetDbId, userPosition.Value);

                // Подтверждаем транзакцию
                transaction.Commit();
            }
            catch
            {
                // Откатываем транзакцию в случае ошибки
                transaction.Rollback();
                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        /// <summary>
        /// Получает позицию пользователя в списке.
        /// </summary>
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
            if (result != null && int.TryParse(result.ToString(), out int position))
                return position;
            else
                return null;
        }

        /// <summary>
        /// Обновляет позицию пользователя в списке.
        /// </summary>
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
        /// Получает всех пользователей в указанном списке.
        /// </summary>
        /// <param name="listId">Идентификатор списка.</param>
        /// <returns>Список участников (Participant) в порядке их позиций.</returns>
        /// <exception cref="InvalidOperationException">Бросается, если список не существует.</exception>
        public async Task<List<Participant>> GetAllUsersInList(long listId)
        {
            var participants = new List<Participant>();

            await using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            // Проверяем существование списка
            var checkListQuery = "SELECT COUNT(1) FROM lists WHERE id = @list_id;";
            await using (var checkListCmd = new SQLiteCommand(checkListQuery, connection))
            {
                checkListCmd.Parameters.AddWithValue("@list_id", listId);
                var listExists = Convert.ToInt32(await checkListCmd.ExecuteScalarAsync()) > 0;
                if (!listExists)
                    throw new InvalidOperationException($"List with ID {listId} does not exist.");
            }

            // Запрос для получения пользователей в списке с сортировкой по позиции
            var selectUsersQuery = @"
                SELECT u.telegram_id, u.full_name
                FROM list_members lm
                JOIN users u ON lm.user_id = u.id
                WHERE lm.list_id = @list_id
                ORDER BY lm.position ASC;
            ";

            await using var command = new SQLiteCommand(selectUsersQuery, connection);
            command.Parameters.AddWithValue("@list_id", listId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var telegramId = reader.GetInt64(0); // telegram_id
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
        /// Перемещает пользователя в конец списка. Если пользователь не находится в списке, он будет добавлен.
        /// </summary>
        /// <param name="listId">Идентификатор списка.</param>
        /// <param name="telegramId">Telegram ID пользователя.</param>
        /// <returns>True, если операция успешна; иначе, false.</returns>
        public async Task<bool> MoveUserToEndOfListAsync(long listId, long telegramId)
        {
            if (listId <= 0)
                throw new ArgumentException("List ID must be a positive number.", nameof(listId));

            await using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Проверяем существование списка
                var checkListQuery = "SELECT COUNT(1) FROM lists WHERE id = @list_id;";
                await using (var checkListCmd = new SQLiteCommand(checkListQuery, connection, transaction))
                {
                    checkListCmd.Parameters.AddWithValue("@list_id", listId);
                    var listExists = Convert.ToInt32(await checkListCmd.ExecuteScalarAsync()) > 0;
                    if (!listExists)
                        throw new InvalidOperationException($"List with ID {listId} does not exist.");
                }

                // Проверяем существование пользователя
                var checkUserQuery = "SELECT COUNT(1) FROM users WHERE telegram_id = @telegramId;";
                await using (var checkUserCmd = new SQLiteCommand(checkUserQuery, connection, transaction))
                {
                    checkUserCmd.Parameters.AddWithValue("@telegramId", telegramId);
                    var userExists = Convert.ToInt32(await checkUserCmd.ExecuteScalarAsync()) > 0;
                    if (!userExists)
                        throw new InvalidOperationException($"User with Telegram ID {telegramId} does not exist.");
                }

                // Получаем user_id из telegram_id
                var userId = await GetUserIdByTelegramIdAsync(telegramId);

                // Получаем максимальную позицию в списке
                var getMaxPositionQuery = @"
                            SELECT IFNULL(MAX(position), 0)
                            FROM list_members
                            WHERE list_id = @list_id;
                        ";
                int newPosition = 1;
                using (var maxPosCmd = new SQLiteCommand(getMaxPositionQuery, connection, transaction))
                {
                    maxPosCmd.Parameters.AddWithValue("@list_id", listId);
                    var maxPosResult = await maxPosCmd.ExecuteScalarAsync();
                    if (maxPosResult != null && int.TryParse(maxPosResult.ToString(), out int maxPosition))
                    {
                        newPosition = maxPosition + 1;
                    }
                }

                // Проверяем, находится ли пользователь уже в списке
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
                    if (result != null && int.TryParse(result.ToString(), out int pos))
                    {
                        existingPosition = pos;
                    }
                }

                if (existingPosition.HasValue)
                {
                    // Пользователь уже в списке, обновляем позицию
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
                    // Пользователь не в списке, добавляем его
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
                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();
                        if (rowsAffected <= 0)
                        {
                            // Не удалось добавить пользователя
                            return false;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        await Console.Error.WriteLineAsync($"SQLite error: {ex.Message}");
                        return false;
                    }
                }

                // Подтверждаем транзакцию
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Откатываем транзакцию в случае ошибки
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