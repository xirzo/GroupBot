using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SQLite;
using GroupBot.Library.Models;

namespace GroupBot.Library.Services.Database;

[DbConfigurationType(typeof(SQLiteConfiguration))]
public sealed class BotDbContext : DbContext
{
    private readonly string _dbPath;

    public BotDbContext(string dbPath) 
        : base(CreateConnection(dbPath), true)
    {
        _dbPath = dbPath;
        Users = Set<User>();
        Lists = Set<ChatList>();
        ListMembers = Set<ListMember>();
        Admins = Set<Admin>();
        LowPriorityUsers = Set<LowPriorityUser>();
        
        System.Data.Entity.Database.SetInitializer<BotDbContext>(null);
    }

    private static SQLiteConnection CreateConnection(string dbPath)
    {
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = dbPath,
            ForeignKeys = true
        };
        
        return new SQLiteConnection(builder.ConnectionString);
    }

    public void EnsureDatabaseCreated()
    {
        if (!File.Exists(_dbPath))
        {
            SQLiteConnection.CreateFile(_dbPath);
        }

        using var connection = new SQLiteConnection(Database.Connection.ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                telegram_id INTEGER NOT NULL UNIQUE,
                full_name TEXT NOT NULL,
                created_at DATETIME NOT NULL
            );

            CREATE TABLE IF NOT EXISTS lists (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                list_name TEXT NOT NULL,
                created_at DATETIME NOT NULL
            );

            CREATE TABLE IF NOT EXISTS list_members (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                list_id INTEGER NOT NULL,
                user_id INTEGER NOT NULL,
                position INTEGER NOT NULL,
                inserted_at DATETIME NOT NULL,
                FOREIGN KEY (list_id) REFERENCES lists(id) ON DELETE CASCADE,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
                UNIQUE (list_id, position),
                UNIQUE (list_id, user_id)
            );

            CREATE TABLE IF NOT EXISTS admins (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS low_priority_users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
                UNIQUE (user_id)
            );

            CREATE INDEX IF NOT EXISTS idx_users_telegram_id ON users(telegram_id);
            CREATE INDEX IF NOT EXISTS idx_list_members_list_position ON list_members(list_id, position);
            CREATE INDEX IF NOT EXISTS idx_list_members_list_user ON list_members(list_id, user_id);
            CREATE INDEX IF NOT EXISTS idx_low_priority_users_user_id ON low_priority_users(user_id);
        ";

        command.ExecuteNonQuery();
    }

    public DbSet<User> Users { get; init; }
    public DbSet<ChatList> Lists { get; init; }
    public DbSet<ListMember> ListMembers { get; init; }
    public DbSet<Admin> Admins { get; init; }
    public DbSet<LowPriorityUser> LowPriorityUsers { get; init; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("");

        modelBuilder.Entity<User>()
            .ToTable("users")
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .HasColumnName("id")
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        modelBuilder.Entity<User>()
            .Property(u => u.TelegramId)
            .HasColumnName("telegram_id")
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        modelBuilder.Entity<ChatList>()
            .ToTable("lists")
            .HasKey(l => l.Id);

        modelBuilder.Entity<ChatList>()
            .Property(l => l.Id)
            .HasColumnName("id")
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        modelBuilder.Entity<ChatList>()
            .Property(l => l.Name)
            .HasColumnName("list_name")
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<ChatList>()
            .Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        modelBuilder.Entity<ChatList>()
            .HasMany(l => l.Members)
            .WithRequired(m => m.List)
            .HasForeignKey(m => m.ListId)
            .WillCascadeOnDelete(true);

        modelBuilder.Entity<ListMember>()
            .ToTable("list_members")
            .HasKey(lm => lm.Id);

        modelBuilder.Entity<ListMember>()
            .Property(lm => lm.Id)
            .HasColumnName("id")
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        modelBuilder.Entity<ListMember>()
            .Property(lm => lm.ListId)
            .HasColumnName("list_id")
            .IsRequired();

        modelBuilder.Entity<ListMember>()
            .Property(lm => lm.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        modelBuilder.Entity<ListMember>()
            .Property(lm => lm.Position)
            .HasColumnName("position")
            .IsRequired();

        modelBuilder.Entity<ListMember>()
            .Property(lm => lm.InsertedAt)
            .HasColumnName("inserted_at")
            .HasColumnType("datetime")
            .IsRequired();

        modelBuilder.Entity<ListMember>()
            .HasRequired(lm => lm.List)
            .WithMany(l => l.Members)
            .HasForeignKey(lm => lm.ListId)
            .WillCascadeOnDelete(true);

        modelBuilder.Entity<ListMember>()
            .HasRequired(lm => lm.User)
            .WithMany(u => u.ListMemberships)
            .HasForeignKey(lm => lm.UserId)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<Admin>()
            .ToTable("admins")
            .HasKey(a => a.Id);

        modelBuilder.Entity<Admin>()
            .Property(a => a.Id)
            .HasColumnName("id")
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        modelBuilder.Entity<Admin>()
            .Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        modelBuilder.Entity<Admin>()
            .HasRequired(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .WillCascadeOnDelete(false);

        // Configure LowPriorityUsers entity
        modelBuilder.Entity<LowPriorityUser>()
            .ToTable("low_priority_users")
            .HasKey(lpu => lpu.Id);

        modelBuilder.Entity<LowPriorityUser>()
            .Property(lpu => lpu.Id)
            .HasColumnName("id")
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        modelBuilder.Entity<LowPriorityUser>()
            .Property(lpu => lpu.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        modelBuilder.Entity<LowPriorityUser>()
            .HasRequired(lpu => lpu.User)
            .WithMany()
            .HasForeignKey(lpu => lpu.UserId)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.TelegramId)
            .IsUnique();

        modelBuilder.Entity<ListMember>()
            .HasIndex(lm => new { lm.ListId, lm.Position })
            .IsUnique();

        modelBuilder.Entity<ListMember>()
            .HasIndex(lm => new { lm.ListId, lm.UserId })
            .IsUnique();

        modelBuilder.Entity<LowPriorityUser>()
            .HasIndex(lpu => lpu.UserId)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}