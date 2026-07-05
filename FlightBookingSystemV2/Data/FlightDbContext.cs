using Microsoft.EntityFrameworkCore;
using FlightBookingSystem.Models.Entities;

namespace FlightBookingSystem.Data
{
    public class FlightDbContext : DbContext
    {
        public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin>    Admins    { get; set; }
        public DbSet<Flight>   Flights   { get; set; }
        public DbSet<Booking>  Bookings  { get; set; }
        public DbSet<CheckIn>  CheckIns  { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── Unique indexes ──────────────────────────────────────────────
            modelBuilder.Entity<Customer>().HasIndex(c => c.Username).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
            modelBuilder.Entity<Admin>().HasIndex(a => a.Username).IsUnique();
            modelBuilder.Entity<Flight>().HasIndex(f => f.FlightNumber).IsUnique();
            modelBuilder.Entity<Booking>().HasIndex(b => b.ReferenceNumber).IsUnique();
            modelBuilder.Entity<CheckIn>().HasIndex(c => c.CheckInReference).IsUnique();

            // ── Relationships ───────────────────────────────────────────────
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Flight)
                .WithMany(f => f.Bookings)
                .HasForeignKey(b => b.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CheckIn>()
                .HasOne(ci => ci.Booking)
                .WithOne(b => b.CheckIn)
                .HasForeignKey<CheckIn>(ci => ci.BookingId);

            
        }
    }
}
