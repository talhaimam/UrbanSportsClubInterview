using InterviewService.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewService
{
    public class DbContext : Fitogram.EntityFrameworkCore.DbContext
    {
        public DbContextOptions<DbContext> Options { get; }
        public Configuration Configuration { get; }

        public DbContext(DbContextOptions<DbContext> options, Configuration configuration)
            : base(options)
        {
            this.Options = options;
            this.Configuration = configuration;
        }

        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Models.External.Event> Events { get; set; }
        public virtual DbSet<Models.External.Customer> Customers { get; set; }
        public virtual DbSet<Models.External.Provider> Providers { get; set; }
        public virtual DbSet<Models.External.Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
