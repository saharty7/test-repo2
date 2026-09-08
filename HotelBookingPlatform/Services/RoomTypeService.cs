using HotelBookingPlatform.Common;
using HotelBookingPlatform.Data;
using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Models;
using HotelBookingPlatform.Services.Interfaces;

namespace HotelBookingPlatform.Services;

public class RoomTypeService : IRoomTypeService
{
    private readonly AppDbContext _db;

    public RoomTypeService(AppDbContext db)
    {
        _db = db;
    }

    public List<RoomTypeResponse> GetAll()
    {
        return _db.RoomTypes
            .OrderBy(rt => rt.Name)
            .ToList()
            .Select(ToResponse)
            .ToList();
    }

    public RoomTypeResponse GetById(int id)
    {
        var roomType = FindRoomTypeOrThrow(id);
        return ToResponse(roomType);
    }

    public RoomTypeResponse Create(CreateRoomTypeRequest request)
    {
        var name = request.Name.Trim();
        if (_db.RoomTypes.Any(rt => rt.Name.ToLower() == name.ToLower()))
        {
            throw new ConflictException($"A room type named '{name}' already exists.");
        }

        var amenityIds = request.AmenityIds.Distinct().ToList();
        var existingAmenityIds = _db.Amenities.Select(a => a.Id).ToHashSet();
        var invalidAmenityIds = amenityIds.Where(id => !existingAmenityIds.Contains(id)).ToList();
        if (invalidAmenityIds.Count > 0)
        {
            throw new BadRequestException($"Unknown amenity id(s): {string.Join(", ", invalidAmenityIds)}.");
        }

        var roomType = new RoomType
        {
            Name = name,
            PricePerNight = request.PricePerNight,
            MaxGuests = request.MaxGuests,
            AmenityIds = amenityIds.ToArray()
        };
        _db.RoomTypes.Add(roomType);
        _db.SaveChanges();
        return ToResponse(roomType);
    }

    public List<ReviewResponse> GetReviews(int roomTypeId)
    {
        var roomType = FindRoomTypeOrThrow(roomTypeId);

        var roomIds = _db.Rooms.Where(r => r.RoomTypeId == roomTypeId).Select(r => r.Id).ToHashSet();
        var bookingIds = _db.Bookings.Where(b => roomIds.Contains(b.RoomId)).Select(b => b.Id).ToHashSet();

        return _db.Reviews
            .Where(r => bookingIds.Contains(r.BookingId))
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToList()
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                BookingId = r.BookingId,
                RoomTypeId = roomType.Id,
                RoomTypeName = roomType.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAtUtc = r.CreatedAtUtc
            })
            .ToList();
    }

    private RoomType FindRoomTypeOrThrow(int id)
    {
        return _db.RoomTypes.FirstOrDefault(rt => rt.Id == id)
            ?? throw new NotFoundException($"Room type with id {id} was not found.");
    }

    private RoomTypeResponse ToResponse(RoomType roomType)
    {
        var roomIds = _db.Rooms.Where(r => r.RoomTypeId == roomType.Id).Select(r => r.Id).ToHashSet();
        var bookingIds = _db.Bookings.Where(b => roomIds.Contains(b.RoomId)).Select(b => b.Id).ToHashSet();
        var ratings = _db.Reviews.Where(r => bookingIds.Contains(r.BookingId)).Select(r => r.Rating).ToList();

        return new RoomTypeResponse
        {
            Id = roomType.Id,
            Name = roomType.Name,
            PricePerNight = roomType.PricePerNight,
            MaxGuests = roomType.MaxGuests,
            Amenities = _db.Amenities
                .Where(a => roomType.AmenityIds.Contains(a.Id))
                .OrderBy(a => a.Name)
                .Select(a => new AmenityResponse { Id = a.Id, Name = a.Name })
                .ToList(),
            AverageRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 2) : null,
            ReviewCount = ratings.Count
        };
    }
}
