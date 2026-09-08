using HotelBookingPlatform.DTOs;

namespace HotelBookingPlatform.Services.Interfaces;

public interface IBookingService
{
    BookingResponse Create(CreateBookingRequest request);
    BookingResponse GetById(int id);
    BookingResponse Confirm(int id);
    BookingResponse Cancel(int id);
    BookingResponse CheckIn(int id);
    BookingResponse CheckOut(int id);
}
