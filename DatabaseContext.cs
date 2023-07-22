using IBCVideoCourseTGBot.Models;
using Microsoft.EntityFrameworkCore;

namespace IBCVideoCourseTGBot;

public sealed class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; } = default!;
    
    public DatabaseContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { 
        optionsBuilder.UseSqlite("Data Source=database.db");
    }
}