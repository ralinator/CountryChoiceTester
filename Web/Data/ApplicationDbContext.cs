using Microsoft.EntityFrameworkCore;

namespace Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    public DbSet<GameRecord> GameRecords => Set<GameRecord>();
    public DbSet<GameState> GameStates => Set<GameState>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameRecord>(
            record =>
            {
                record.HasKey(x => x.Id);
                record.Property(x => x.Id).ValueGeneratedOnAdd();
            });
        modelBuilder.Entity<GameState>(
            record =>
            {
                record.HasKey(x => x.Id);
                record.Property(x => x.Id).ValueGeneratedOnAdd();
            });
        base.OnModelCreating(modelBuilder);
    }
}
