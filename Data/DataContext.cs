using Microsoft.EntityFrameworkCore;

namespace SafeVault;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
}