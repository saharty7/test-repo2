using HotelBookingPlatform.Common;
using HotelBookingPlatform.Data;
using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Models;
using HotelBookingPlatform.Services.Interfaces;

namespace HotelBookingPlatform.Services;

public class AmenityService : IAmenityService
{
    private readonly AppDbContext _db;

    public AmenityService(AppDbContext db)
    {
        _db = db;
    }

    public List<AmenityResponse> GetAll()
    {
        return _db.Amenities
            .OrderBy(a => a.Name)
            .Select(ToResponse)
            .ToList();
    }

    public AmenityResponse Create(CreateAmenityRequest request)
    {
        var name = request.Name.Trim();
        if (_db.Amenities.Any(a => a.Name.ToLower() == name.ToLower()))
        {
            throw new ConflictException($"An amenity named '{name}' already exists.");
        }

        var amenity = new Amenity { Name = name };
        _db.Amenities.Add(amenity);
        _db.SaveChanges();
        return ToResponse(amenity);
    }

    private static AmenityResponse ToResponse(Amenity amenity) => new()
    {
        Id = amenity.Id,
        Name = amenity.Name
    };
}
