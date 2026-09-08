namespace HotelBookingPlatform.Models;

public class RoomType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public int[] AmenityIds { get; set; } = Array.Empty<int>();
}
