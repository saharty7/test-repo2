namespace HotelBookingPlatform.DTOs;

public class OccupancyReportResponse
{
    public int TotalActiveRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public double OccupancyPercentage { get; set; }
}

public class RoomTypeRatingResponse
{
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
