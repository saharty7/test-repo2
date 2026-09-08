using HotelBookingPlatform.DTOs;

namespace HotelBookingPlatform.Services.Interfaces;

public interface IRoomTypeService
{
    List<RoomTypeResponse> GetAll();
    RoomTypeResponse GetById(int id);
    RoomTypeResponse Create(CreateRoomTypeRequest request);
    List<ReviewResponse> GetReviews(int roomTypeId);
}
