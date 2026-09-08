using HotelBookingPlatform.Common;
using HotelBookingPlatform.Models;

namespace HotelBookingPlatform.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.RoomTypes.Any())
        {
            return;
        }

        var wifi = new Amenity { Name = "Free WiFi" };
        var pool = new Amenity { Name = "Pool View" };
        var parking = new Amenity { Name = "Free Parking" };
        var breakfast = new Amenity { Name = "Breakfast Included" };
        var minibar = new Amenity { Name = "Minibar" };
        db.Amenities.AddRange(wifi, pool, parking, breakfast, minibar);
        db.SaveChanges();

        var standard = new RoomType
        {
            Name = "Standard",
            PricePerNight = 60m,
            MaxGuests = 2,
            AmenityIds = new[] { wifi.Id }
        };
        var deluxe = new RoomType
        {
            Name = "Deluxe",
            PricePerNight = 110m,
            MaxGuests = 3,
            AmenityIds = new[] { wifi.Id, parking.Id, breakfast.Id }
        };
        var suite = new RoomType
        {
            Name = "Suite",
            PricePerNight = 220m,
            MaxGuests = 4,
            AmenityIds = new[] { wifi.Id, pool.Id, parking.Id, breakfast.Id, minibar.Id }
        };
        db.RoomTypes.AddRange(standard, deluxe, suite);
        db.SaveChanges();

        db.Rooms.AddRange(
            new Room { Number = "101", RoomTypeId = standard.Id, IsActive = true },
            new Room { Number = "102", RoomTypeId = standard.Id, IsActive = true },
            new Room { Number = "103", RoomTypeId = standard.Id, IsActive = true },
            new Room { Number = "201", RoomTypeId = deluxe.Id, IsActive = true },
            new Room { Number = "202", RoomTypeId = deluxe.Id, IsActive = true },
            new Room { Number = "301", RoomTypeId = suite.Id, IsActive = true }
        );

        var (hash, salt) = PasswordHasher.Hash("Admin@123");
        db.StaffUsers.Add(new StaffUser
        {
            Username = "admin",
            PasswordHash = hash,
            PasswordSalt = salt
        });

        db.SaveChanges();
    }
}
