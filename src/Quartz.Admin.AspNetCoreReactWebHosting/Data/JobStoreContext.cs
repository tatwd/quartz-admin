using Microsoft.EntityFrameworkCore;

namespace Quartz.Admin.AspNetCoreReactWebHosting.Data
{
    public class JobStoreContext : DbContext
    {
        public JobStoreContext(DbContextOptions<JobStoreContext> options) : base(options) { }

        public DbSet<JobSetting> JobSettings { get; set; }
        public DbSet<JobLog> JobLogs { get; set; }
    }
}