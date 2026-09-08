namespace HotelBookingPlatform.Models;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public bool IsActive { get; set; } = true;
}
