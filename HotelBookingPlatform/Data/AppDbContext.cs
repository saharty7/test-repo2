using HotelBookingPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoomType>()
            .Property(rt => rt.AmenityIds)
            .HasColumnType("integer[]");

        modelBuilder.Entity<RoomType>()
            .HasIndex(rt => rt.Name)
            .IsUnique();

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.Number)
            .IsUnique();

        modelBuilder.Entity<Amenity>()
            .HasIndex(a => a.Name)
            .IsUnique();

        modelBuilder.Entity<Guest>()
            .HasIndex(g => g.Email)
            .IsUnique();

        modelBuilder.Entity<StaffUser>()
            .HasIndex(s => s.Username)
            .IsUnique();

        modelBuilder.Entity<Review>()
            .HasIndex(r => r.BookingId)
            .IsUnique();
    }
}
