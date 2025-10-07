using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Server.Models;

namespace Server.Persistence;

public class WssDbContext(DbContextOptions options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Consultant> Consultants { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Round> Rounds { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<Tender> Tenders { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbOptions = configuration.GetSection("Database");

        var dbHost = dbOptions.GetValue<string>("Host");
        var dbPort = dbOptions.GetValue<string>("Port");
        var dbName = dbOptions.GetValue<string>("Name");
        var dbUser = dbOptions.GetValue<string>("User");
        var dbPass = dbOptions.GetValue<string>("Pass");

        var dbConnectionString = $"Host={dbHost};Port={dbPort};Db={dbName};Username={dbUser};Password={dbPass}";

        optionsBuilder.UseNpgsql(dbConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("companies");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.Property(e => e.Treasury).HasColumnType("integer").HasDefaultValue(1000000);

            e.HasOne(e => e.Player)
                .WithOne(e => e.Company)
                .HasForeignKey<Company>(e => e.PlayerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(e => e.Employees)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.CompanyId);
        });

        modelBuilder.Entity<Consultant>(e =>
        {
            e.ToTable("consultants");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");

            e.HasOne(e => e.Game)
                .WithMany(g => g.Consultants)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            e.OwnsMany(e => e.Skills, builder => builder.ToJson());
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("employees");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");

            e.HasOne(e => e.Game)
                .WithMany() // pas de nav Game.Employees
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            e.OwnsMany(e => e.Skills, builder => builder.ToJson());
        });

        modelBuilder.Entity<Game>(e =>
        {
            e.ToTable("games");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.Property(e => e.Rounds).HasColumnType("integer");
            e.Property(e => e.Status)
                .HasColumnType("varchar(255)")
                .HasDefaultValue(GameStatus.Waiting)
                .HasConversion(new EnumToStringConverter<GameStatus>());

            e.HasMany(g => g.Players)
                .WithOne(p => p.Game)
                .HasForeignKey(p => p.GameId);

            e.HasMany(g => g.Consultants)
                .WithOne(c => c.Game)
                .HasForeignKey(c => c.GameId);

            e.HasMany(g => g.RoundsCollection)
                .WithOne(r => r.Game)
                .HasForeignKey(r => r.GameId);

            e.HasMany(g => g.Tenders)
                .WithOne(t => t.Game)
                .HasForeignKey(t => t.GameId);
        });

        modelBuilder.Entity<Player>(e =>
        {
            e.ToTable("players");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");

            e.HasOne(e => e.Game)
                .WithMany(g => g.Players)
                .HasForeignKey(e => e.GameId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(e => e.Company)
                .WithOne(c => c.Player)
                .HasForeignKey<Company>(c => c.PlayerId);
        });

        modelBuilder.Entity<Round>(e =>
        {
            e.ToTable("rounds");
            e.HasKey(e => e.Id);
            e.Property(e => e.Order).HasColumnType("integer");

            e.HasOne(e => e.Game)
                .WithMany(g => g.RoundsCollection)
                .HasForeignKey(e => e.GameId);

            e.OwnsMany(e => e.Actions, builder => builder.ToJson());
        });

        modelBuilder.Entity<Skill>(e =>
        {
            e.ToTable("skills");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasData(
                new Skill("HTML") { Id = 1 },
                new Skill("CSS") { Id = 2 },
                new Skill("JavaScript") { Id = 3 },
                new Skill("TypeScript") { Id = 4 },
                new Skill("React") { Id = 5 },
                new Skill("Angular") { Id = 6 },
                new Skill("Vue.js") { Id = 7 },
                new Skill("Node.js") { Id = 8 },
                new Skill("Express.js") { Id = 9 },
                new Skill("ASP.NET Core") { Id = 10 },
                new Skill("Ruby on Rails") { Id = 11 },
                new Skill("Django") { Id = 12 },
                new Skill("Flask") { Id = 13 },
                new Skill("PHP") { Id = 14 },
                new Skill("Laravel") { Id = 15 },
                new Skill("Spring Boot") { Id = 16 },
                new Skill("SQL") { Id = 17 },
                new Skill("NoSQL") { Id = 18 },
                new Skill("GraphQL") { Id = 19 },
                new Skill("REST APIs") { Id = 20 }
            );
        });

        // ----- Tender (nouveau) -----
        modelBuilder.Entity<Tender>(e =>
        {
            e.ToTable("tenders");
            e.HasKey(x => x.Id);

            // FK → Game (pour Game.Tenders)
            e.HasOne(x => x.Game)
                .WithMany(g => g.Tenders)
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK → Company
            e.HasOne(x => x.Company)
                .WithMany() // si tu ajoutes ICollection<Tender> Tenders dans Company: .WithMany(c => c.Tenders)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Skills en JSON (même pattern que Employee/Consultant)
            e.OwnsMany(x => x.Skills, builder => builder.ToJson());
        });
    }
}
