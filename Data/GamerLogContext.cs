namespace GamerLog.Data;
using Microsoft.EntityFrameworkCore;
using GamerLog.Entities;

public class GamerLogContext : DbContext
{
    public GamerLogContext(DbContextOptions<GamerLogContext> options) : base(options) { }
    
    public DbSet<Game> Games { get; set; }
    
    public DbSet<Genre> Genres { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>()
            .UseTptMappingStrategy();
        
        modelBuilder.Entity<DigitalGame>().ToTable("DigitalGames");
    }
}