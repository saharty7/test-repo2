using HotelBookingPlatform.Common;
using HotelBookingPlatform.Data;
using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Models;
using HotelBookingPlatform.Services.Interfaces;

namespace HotelBookingPlatform.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _db;

    public RoomService(AppDbContext db)
    {
        _db = db;
    }

    public List<RoomResponse> GetAll()
    {
        return _db.Rooms
            .OrderBy(r => r.Number)
            .ToList()
            .Select(ToResponse)
            .ToList();
    }

    public RoomResponse GetById(int id)
    {
        return ToResponse(FindRoomOrThrow(id));
    }

    public RoomResponse Create(CreateRoomRequest request)
    {
        var roomType = _db.RoomTypes.FirstOrDefault(rt => rt.Id == request.RoomTypeId)
            ?? throw new NotFoundException($"Room type with id {request.RoomTypeId} was not found.");

        var number = request.Number.Trim();
        if (_db.Rooms.Any(r => r.Number.ToLower() == number.ToLower()))
        {
            throw new ConflictException($"A room numbered '{number}' already exists.");
        }

        var room = new Room
        {
            Number = number,
            RoomTypeId = roomType.Id,
            IsActive = true
        };
        _db.Rooms.Add(room);
        _db.SaveChanges();
        return ToResponse(room);
    }

    public RoomResponse SetActive(int id, bool isActive)
    {
        var room = FindRoomOrThrow(id);
        room.IsActive = isActive;
        _db.SaveChanges();
        return ToResponse(room);
    }

    public List<RoomResponse> GetAvailableRooms(DateOnly checkIn, DateOnly checkOut, int? roomTypeId)
    {
        if (checkOut <= checkIn)
        {
            throw new BadRequestException("Check-out date must be after the check-in date.");
        }

        if (roomTypeId.HasValue && _db.RoomTypes.All(rt => rt.Id != roomTypeId.Value))
        {
            throw new NotFoundException($"Room type with id {roomTypeId.Value} was not found.");
        }

        var candidateRooms = _db.Rooms.Where(r => r.IsActive);
        if (roomTypeId.HasValue)
        {
            candidateRooms = candidateRooms.Where(r => r.RoomTypeId == roomTypeId.Value);
        }

        var overlappingRoomIds = _db.Bookings
            .Where(b => b.Status != BookingStatus.Cancelled && checkIn < b.CheckOutDate && checkOut > b.CheckInDate)
            .Select(b => b.RoomId)
            .ToHashSet();

        return candidateRooms
            .Where(r => !overlappingRoomIds.Contains(r.Id))
            .OrderBy(r => r.Number)
            .ToList()
            .Select(ToResponse)
            .ToList();
    }

    private Room FindRoomOrThrow(int id)
    {
        return _db.Rooms.FirstOrDefault(r => r.Id == id)
            ?? throw new NotFoundException($"Room with id {id} was not found.");
    }

    private RoomResponse ToResponse(Room room)
    {
        var roomType = _db.RoomTypes.First(rt => rt.Id == room.RoomTypeId);
        return new RoomResponse
        {
            Id = room.Id,
            Number = room.Number,
            RoomTypeId = room.RoomTypeId,
            RoomTypeName = roomType.Name,
            PricePerNight = roomType.PricePerNight,
            IsActive = room.IsActive
        };
    }
}
