using Microsoft.EntityFrameworkCore;
using WebApi.Models;

public class BddContext : DbContext
{
    public BddContext(DbContextOptions<BddContext> options)
        : base(options)
    {
    }

    // Default constructor for migrations or tests
    public BddContext() { }

    // DbSet declarations
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Film> Films { get; set; } = null!;
    public DbSet<Favourite> Favourites { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Use SQLite as a fallback if no configuration is provided
        if (!options.IsConfigured)
        {
            options.UseSqlite("Data Source=Bdd.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships
        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Film)
            .WithMany()
            .HasForeignKey(f => f.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraints
        modelBuilder.Entity<Film>()
            .HasIndex(f => f.Imdb)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Pseudo)
            .IsUnique();

        // Configure required properties and max lengths
        modelBuilder.Entity<Film>()
            .Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .Property(u => u.Pseudo)
            .IsRequired()
            .HasMaxLength(100);
    }
}
