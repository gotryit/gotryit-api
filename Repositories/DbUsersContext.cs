using Microsoft.EntityFrameworkCore;

public class DbUsersContext : DbContext
{
    public DbUsersContext(DbContextOptions<DbUsersContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users { get; set; }
}