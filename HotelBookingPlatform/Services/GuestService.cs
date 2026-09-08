using HotelBookingPlatform.Common;
using HotelBookingPlatform.Data;
using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Services.Interfaces;

namespace HotelBookingPlatform.Services;

public class GuestService : IGuestService
{
    private readonly AppDbContext _db;

    public GuestService(AppDbContext db)
    {
        _db = db;
    }

    public List<GuestResponse> GetAll()
    {
        return _db.Guests
            .OrderBy(g => g.Name)
            .Select(g => new GuestResponse { Id = g.Id, Name = g.Name, Email = g.Email, Phone = g.Phone })
            .ToList();
    }

    public GuestResponse GetById(int id)
    {
        var guest = _db.Guests.FirstOrDefault(g => g.Id == id)
            ?? throw new NotFoundException($"Guest with id {id} was not found.");
        return new GuestResponse { Id = guest.Id, Name = guest.Name, Email = guest.Email, Phone = guest.Phone };
    }

    public List<BookingResponse> GetBookingHistory(int guestId)
    {
        var guest = _db.Guests.FirstOrDefault(g => g.Id == guestId)
            ?? throw new NotFoundException($"Guest with id {guestId} was not found.");

        return _db.Bookings
            .Where(b => b.GuestId == guest.Id)
            .OrderByDescending(b => b.CheckInDate)
            .ToList()
            .Select(b =>
            {
                var room = _db.Rooms.First(r => r.Id == b.RoomId);
                var roomType = _db.RoomTypes.First(rt => rt.Id == room.RoomTypeId);
                return new BookingResponse
                {
                    Id = b.Id,
                    GuestId = guest.Id,
                    GuestName = guest.Name,
                    GuestEmail = guest.Email,
                    RoomId = room.Id,
                    RoomNumber = room.Number,
                    RoomTypeId = roomType.Id,
                    RoomTypeName = roomType.Name,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Nights = b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    CreatedAtUtc = b.CreatedAtUtc
                };
            })
            .ToList();
    }
}
