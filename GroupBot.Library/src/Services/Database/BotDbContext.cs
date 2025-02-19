using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SQLite;
using GroupBot.Library.Models;

namespace GroupBot.Library.Services.Database;

[DbConfigurationType(typeof(SQLiteConfiguration))]
public sealed class BotDbContext : DbContext
{
    public BotDbContext(string dbPath) 
        : base(CreateConnection(dbPath), true)
    {
        Users = Set<User>();
        Lists = Set<ChatList>();
        ListMembers = Set<ListMember>();
        Admins = Set<Admin>();

        System.Data.Entity.Database.SetInitializer<BotDbContext>(null);
    }

    private static SQLiteConnection CreateConnection(string dbPath)
    {
        return new SQLiteConnection
        {
            ConnectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                ForeignKeys = true
            }.ConnectionString
        };
    }

    public DbSet<User> Users { get; init; }
    public DbSet<ChatList> Lists { get; init; }
    public DbSet<ListMember> ListMembers { get; init; }
    public DbSet<Admin> Admins { get; init; }

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

        modelBuilder.Entity<User>()
            .HasIndex(u => u.TelegramId)
            .IsUnique();

        modelBuilder.Entity<ListMember>()
            .HasIndex(lm => new { lm.ListId, lm.Position })
            .IsUnique();

        modelBuilder.Entity<ListMember>()
            .HasIndex(lm => new { lm.ListId, lm.UserId })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
