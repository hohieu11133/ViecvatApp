using Microsoft.EntityFrameworkCore;
using UserService.Entities;

namespace UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        // Khai báo các bảng sẽ có trong DB
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Worker> Workers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Đảm bảo số điện thoại là duy nhất
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Phone)
                .IsUnique();
        }
    }
}