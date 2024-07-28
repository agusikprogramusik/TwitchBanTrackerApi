using Microsoft.EntityFrameworkCore;
using TwitchBanTrackerApi.Common.Entities;

namespace TwitchBanTrackerApi.Infrastructure.Persistence
{
    public class TwitchBanTrackerDbContext : DbContext
    {
        public TwitchBanTrackerDbContext(DbContextOptions<TwitchBanTrackerDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(m => m.Id);
        }
    }
}
