using System.ComponentModel.DataAnnotations;

namespace HotelBookingPlatform.DTOs;

public class CreateReviewRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid BookingId is required.")]
    public int BookingId { get; set; }

    [Required, EmailAddress]
    public string GuestEmail { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}

public class ReviewResponse
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
