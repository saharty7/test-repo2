using System.ComponentModel.DataAnnotations;
using HotelBookingPlatform.Models;

namespace HotelBookingPlatform.DTOs;

public class CreateBookingRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string GuestName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string GuestEmail { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 3)]
    public string GuestPhone { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "A valid RoomId is required.")]
    public int RoomId { get; set; }

    [Required]
    public DateOnly CheckInDate { get; set; }

    [Required]
    public DateOnly CheckOutDate { get; set; }
}

public class BookingResponse
{
    public int Id { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int Nights { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
