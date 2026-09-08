using HotelBookingPlatform.Data;
using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Models;
using HotelBookingPlatform.Services.Interfaces;

namespace HotelBookingPlatform.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public OccupancyReportResponse GetOccupancy()
    {
        var activeRooms = _db.Rooms.Where(r => r.IsActive).ToList();
        var occupiedRoomIds = _db.Bookings
            .Where(b => b.Status == BookingStatus.CheckedIn)
            .Select(b => b.RoomId)
            .ToHashSet();

        var occupiedCount = activeRooms.Count(r => occupiedRoomIds.Contains(r.Id));
        var percentage = activeRooms.Count == 0 ? 0 : Math.Round(occupiedCount * 100.0 / activeRooms.Count, 2);

        return new OccupancyReportResponse
        {
            TotalActiveRooms = activeRooms.Count,
            OccupiedRooms = occupiedCount,
            OccupancyPercentage = percentage
        };
    }

    public List<RoomTypeRatingResponse> GetBestReviewedRoomTypes()
    {
        var roomToRoomType = _db.Rooms.ToDictionary(r => r.Id, r => r.RoomTypeId);
        var bookingToRoomType = _db.Bookings.ToDictionary(b => b.Id, b => roomToRoomType.GetValueOrDefault(b.RoomId));
        var roomTypeNames = _db.RoomTypes.ToDictionary(rt => rt.Id, rt => rt.Name);

        var grouped = _db.Reviews
            .ToList()
            .Where(r => bookingToRoomType.ContainsKey(r.BookingId))
            .GroupBy(r => bookingToRoomType[r.BookingId])
            .Select(g => new RoomTypeRatingResponse
            {
                RoomTypeId = g.Key,
                RoomTypeName = roomTypeNames.GetValueOrDefault(g.Key, "Unknown"),
                AverageRating = Math.Round(g.Average(r => r.Rating), 2),
                ReviewCount = g.Count()
            })
            .OrderByDescending(r => r.AverageRating)
            .ThenByDescending(r => r.ReviewCount)
            .ToList();

        return grouped;
    }
}
