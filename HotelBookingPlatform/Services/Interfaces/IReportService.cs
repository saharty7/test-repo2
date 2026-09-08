using HotelBookingPlatform.DTOs;

namespace HotelBookingPlatform.Services.Interfaces;

public interface IReportService
{
    OccupancyReportResponse GetOccupancy();
    List<RoomTypeRatingResponse> GetBestReviewedRoomTypes();
}
