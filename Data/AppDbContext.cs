
using ExpenseAuthApi.Model;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAuthApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Expense> Expenses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}