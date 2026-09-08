using HotelBookingPlatform.DTOs;

namespace HotelBookingPlatform.Services.Interfaces;

public interface IAmenityService
{
    List<AmenityResponse> GetAll();
    AmenityResponse Create(CreateAmenityRequest request);
}
