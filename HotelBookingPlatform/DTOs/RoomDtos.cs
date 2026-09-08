using System.ComponentModel.DataAnnotations;

namespace HotelBookingPlatform.DTOs;

public class RoomResponse
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; }
}

public class CreateRoomRequest
{
    [Required, StringLength(20, MinimumLength = 1)]
    public string Number { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "A valid RoomTypeId is required.")]
    public int RoomTypeId { get; set; }
}
