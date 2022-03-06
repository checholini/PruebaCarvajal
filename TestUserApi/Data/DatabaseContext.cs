using Microsoft.EntityFrameworkCore;
using TestUserApi.Models;

namespace TestUserApi.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> opts) : base(opts) { }

        public DbSet<UserModel> Users { get; set; } 
        public DbSet<DocTypeModel> DocTypes { get; set; }
    }
}
