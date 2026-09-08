using HotelBookingPlatform.Common;
using HotelBookingPlatform.Data;
using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Services.Interfaces;

namespace HotelBookingPlatform.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

    private readonly AppDbContext _db;
    private readonly StaffTokenStore _tokenStore;

    public AuthService(AppDbContext db, StaffTokenStore tokenStore)
    {
        _db = db;
        _tokenStore = tokenStore;
    }

    public LoginResponse Login(LoginRequest request)
    {
        var username = request.Username.Trim();
        var staffUser = _db.StaffUsers.FirstOrDefault(s => s.Username.ToLower() == username.ToLower());

        var isValid = staffUser is not null &&
            PasswordHasher.Verify(request.Password, staffUser.PasswordHash, staffUser.PasswordSalt);

        if (!isValid)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        var token = _tokenStore.IssueToken(username, TokenLifetime);
        return new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.Add(TokenLifetime)
        };
    }
}
