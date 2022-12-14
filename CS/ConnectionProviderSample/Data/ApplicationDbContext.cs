using Microsoft.EntityFrameworkCore;

namespace ConnectionProviderSample.Data {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
        }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<Report>().ToTable("Report");
        }
    }
}
