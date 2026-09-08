using HotelBookingPlatform.DTOs;

namespace HotelBookingPlatform.Services.Interfaces;

public interface IRoomService
{
    List<RoomResponse> GetAll();
    RoomResponse GetById(int id);
    RoomResponse Create(CreateRoomRequest request);
    RoomResponse SetActive(int id, bool isActive);
    List<RoomResponse> GetAvailableRooms(DateOnly checkIn, DateOnly checkOut, int? roomTypeId);
}
