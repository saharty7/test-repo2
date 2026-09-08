using HotelBookingPlatform.DTOs;

namespace HotelBookingPlatform.Services.Interfaces;

public interface IAuthService
{
    LoginResponse Login(LoginRequest request);
}
