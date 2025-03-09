using Microsoft.EntityFrameworkCore;

namespace SafeVault;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseSqlite("Data Source=SafeVault.db");
    // }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
}