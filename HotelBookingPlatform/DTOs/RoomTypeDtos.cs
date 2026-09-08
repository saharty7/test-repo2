using System.ComponentModel.DataAnnotations;

namespace HotelBookingPlatform.DTOs;

public class RoomTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public List<AmenityResponse> Amenities { get; set; } = new();
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class CreateRoomTypeRequest
{
    [Required, StringLength(60, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal PricePerNight { get; set; }

    [Range(1, 20)]
    public int MaxGuests { get; set; }

    public List<int> AmenityIds { get; set; } = new();
}
