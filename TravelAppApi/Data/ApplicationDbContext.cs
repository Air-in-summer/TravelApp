using Microsoft.EntityFrameworkCore;
using TravelAppApi.Models;

namespace TravelAppApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<Destinations> Destinations { get; set; }
        public DbSet<Places> Places { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Trip: 1-nhiều
            modelBuilder.Entity<User>()
                .HasMany(u => u.Trips)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Destinations - Trip: 1-nhiều
            modelBuilder.Entity<Destinations>()
                .HasMany(d => d.Trips)
                .WithOne(t => t.Destination)
                .HasForeignKey(t => t.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trip - Stop: 1-nhiều
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Stops)
                .WithOne(s => s.Trip)
                .HasForeignKey(s => s.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Places - Stop: 1-nhiều
            modelBuilder.Entity<Places>()
                .HasMany(p => p.Stop)
                .WithOne(s => s.Location)
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trip - UserPreferences: 1-nhiều
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Preferences)
                .WithOne(p => p.Trip)
                .HasForeignKey(p => p.TripId);

            // Places - Review: 1-nhiều
            modelBuilder.Entity<Places>()
                .HasMany(p => p.Review)
                .WithOne(r => r.Place)
                .HasForeignKey(r => r.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Hoặc Restrict

            // User - Review: 1-nhiều
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Định nghĩa kiểu dữ liệu cho các trường decimal
            modelBuilder.Entity<Trip>(e =>
            {
                e.Property(p => p.Budget).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Stop>(e =>
            {
                e.Property(p => p.EstimatedCost).HasColumnType("decimal(18, 2)");
            });
        }

    }
}
