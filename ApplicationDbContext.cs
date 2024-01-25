using Microsoft.EntityFrameworkCore;

namespace PulsenicsApp
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Point> Points { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Point>().ToTable("Points");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      // Specify the connection string
      optionsBuilder.UseSqlite("Data Source=mydatabase.sqlite");
    }
  }
}
