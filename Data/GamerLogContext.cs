namespace GamerLog.Data;
using Microsoft.EntityFrameworkCore;
using GamerLog.Entities;

public class GamerLogContext : DbContext
{
    public GamerLogContext(DbContextOptions<GamerLogContext> options) : base(options) { }
    
    public DbSet<Game> Games { get; set; }
    
    public DbSet<Genre> Genres { get; set; }
    
    public DbSet<Review> Reviews { get; set; }
    
    public DbSet<GameDetails> GameDetails { get; set; }
    
    public DbSet<Like> Likes { get; set; }
}