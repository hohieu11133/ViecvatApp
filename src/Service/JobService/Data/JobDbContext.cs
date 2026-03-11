using JobService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobService.Data
{
    // 1. Class quản lý Database
    public class JobDbContext : DbContext
    {
        public JobDbContext(DbContextOptions<JobDbContext> options) : base(options) { }

        public DbSet<Job> Jobs { get; set; }
    }

    // 2. Factory dành riêng cho Terminal (Chống lỗi lúc gõ lệnh)
    public class JobDbContextFactory : IDesignTimeDbContextFactory<JobDbContext>
    {
        public JobDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<JobDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Cấu hình SQL Server và NetTopologySuite (GPS)
            builder.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.UseNetTopologySuite());

            return new JobDbContext(builder.Options);
        }
    }
}