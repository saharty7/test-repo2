using HotelBookingPlatform.Common;
using HotelBookingPlatform.Data;
using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Models;
using HotelBookingPlatform.Services.Interfaces;

namespace HotelBookingPlatform.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;

    public BookingService(AppDbContext db)
    {
        _db = db;
    }

    public BookingResponse Create(CreateBookingRequest request)
    {
        if (request.CheckOutDate <= request.CheckInDate)
        {
            throw new BadRequestException("Check-out date must be after the check-in date.");
        }

        if (request.CheckInDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            throw new BadRequestException("Check-in date cannot be in the past.");
        }

        var room = _db.Rooms.FirstOrDefault(r => r.Id == request.RoomId)
            ?? throw new NotFoundException($"Room with id {request.RoomId} was not found.");

        if (!room.IsActive)
        {
            throw new BadRequestException("This room is currently not available for booking.");
        }

        var overlaps = _db.Bookings.Any(b =>
            b.RoomId == room.Id &&
            b.Status != BookingStatus.Cancelled &&
            request.CheckInDate < b.CheckOutDate &&
            request.CheckOutDate > b.CheckInDate);
        if (overlaps)
        {
            throw new ConflictException($"Room {room.Number} is already booked for part or all of the requested dates.");
        }

        var email = request.GuestEmail.Trim();
        var guest = _db.Guests.FirstOrDefault(g => g.Email.ToLower() == email.ToLower());
        if (guest is null)
        {
            guest = new Guest
            {
                Name = request.GuestName.Trim(),
                Email = email,
                Phone = request.GuestPhone.Trim()
            };
            _db.Guests.Add(guest);
        }
        else
        {
            guest.Name = request.GuestName.Trim();
            guest.Phone = request.GuestPhone.Trim();
        }

        _db.SaveChanges();

        var roomType = _db.RoomTypes.First(rt => rt.Id == room.RoomTypeId);
        var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;

        var booking = new Booking
        {
            GuestId = guest.Id,
            RoomId = room.Id,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            TotalPrice = nights * roomType.PricePerNight,
            Status = BookingStatus.Requested
        };
        _db.Bookings.Add(booking);
        _db.SaveChanges();

        return ToResponse(booking);
    }

    public BookingResponse GetById(int id)
    {
        return ToResponse(FindBookingOrThrow(id));
    }

    public BookingResponse Confirm(int id)
    {
        var booking = FindBookingOrThrow(id);
        if (booking.Status != BookingStatus.Requested)
        {
            throw new ConflictException($"Only requested bookings can be confirmed. This booking is currently '{booking.Status}'.");
        }

        booking.Status = BookingStatus.Confirmed;
        _db.SaveChanges();
        return ToResponse(booking);
    }

    public BookingResponse Cancel(int id)
    {
        var booking = FindBookingOrThrow(id);
        if (booking.Status is BookingStatus.CheckedIn or BookingStatus.CheckedOut or BookingStatus.Cancelled)
        {
            throw new ConflictException($"A booking that is '{booking.Status}' cannot be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        _db.SaveChanges();
        return ToResponse(booking);
    }

    public BookingResponse CheckIn(int id)
    {
        var booking = FindBookingOrThrow(id);
        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new ConflictException($"Only confirmed bookings can be checked in. This booking is currently '{booking.Status}'.");
        }

        booking.Status = BookingStatus.CheckedIn;
        _db.SaveChanges();
        return ToResponse(booking);
    }

    public BookingResponse CheckOut(int id)
    {
        var booking = FindBookingOrThrow(id);
        if (booking.Status != BookingStatus.CheckedIn)
        {
            throw new ConflictException($"Only checked-in bookings can be checked out. This booking is currently '{booking.Status}'.");
        }

        booking.Status = BookingStatus.CheckedOut;
        _db.SaveChanges();
        return ToResponse(booking);
    }

    private Booking FindBookingOrThrow(int id)
    {
        return _db.Bookings.FirstOrDefault(b => b.Id == id)
            ?? throw new NotFoundException($"Booking with id {id} was not found.");
    }

    private BookingResponse ToResponse(Booking booking)
    {
        var guest = _db.Guests.First(g => g.Id == booking.GuestId);
        var room = _db.Rooms.First(r => r.Id == booking.RoomId);
        var roomType = _db.RoomTypes.First(rt => rt.Id == room.RoomTypeId);

        return new BookingResponse
        {
            Id = booking.Id,
            GuestId = guest.Id,
            GuestName = guest.Name,
            GuestEmail = guest.Email,
            RoomId = room.Id,
            RoomNumber = room.Number,
            RoomTypeId = roomType.Id,
            RoomTypeName = roomType.Name,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            Nights = booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            CreatedAtUtc = booking.CreatedAtUtc
        };
    }
}
