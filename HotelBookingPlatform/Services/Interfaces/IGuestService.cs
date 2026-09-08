using HotelBookingPlatform.DTOs;

namespace HotelBookingPlatform.Services.Interfaces;

public interface IGuestService
{
    List<GuestResponse> GetAll();
    GuestResponse GetById(int id);
    List<BookingResponse> GetBookingHistory(int guestId);
}
