namespace HotelBookingPlatform.Models;

public class Booking
{
    public int Id { get; set; }
    public int GuestId { get; set; }
    public int RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Requested;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
