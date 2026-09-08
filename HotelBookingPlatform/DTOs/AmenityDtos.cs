using System.ComponentModel.DataAnnotations;

namespace HotelBookingPlatform.DTOs;

public class AmenityResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateAmenityRequest
{
    [Required, StringLength(60, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}
