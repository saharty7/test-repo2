using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HotelBookingPlatform.Common;

public class StaffTokenInfo
{
    public required string Username { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}

public class StaffTokenStore
{
    private const string DefaultSigningKey = "hotel-booking-platform-development-signing-key";
    private readonly byte[] _signingKey;

    public StaffTokenStore(IConfiguration configuration)
    {
        var signingKey = configuration["Authentication:SigningKey"] ?? DefaultSigningKey;
        _signingKey = Encoding.UTF8.GetBytes(signingKey);
    }

    public string IssueToken(string username, TimeSpan lifetime)
    {
        var tokenInfo = new StaffTokenInfo
        {
            Username = username,
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime)
        };
        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(tokenInfo));
        var signature = Sign(payload);
        return $"{payload}.{signature}";
    }

    public bool TryGetUsername(string token, out string username)
    {
        username = string.Empty;

        var tokenParts = token.Split('.', 2);
        if (tokenParts.Length != 2 || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(tokenParts[1]),
                Encoding.UTF8.GetBytes(Sign(tokenParts[0]))))
        {
            return false;
        }

        StaffTokenInfo? info;
        try
        {
            info = JsonSerializer.Deserialize<StaffTokenInfo>(Base64UrlDecode(tokenParts[0]));
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }

        if (info is null || string.IsNullOrWhiteSpace(info.Username) || info.ExpiresAtUtc < DateTime.UtcNow)
        {
            return false;
        }

        username = info.Username;
        return true;
    }

    private string Sign(string payload)
    {
        using var hmac = new HMACSHA256(_signingKey);
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var paddedValue = value.Replace('-', '+').Replace('_', '/');
        paddedValue = paddedValue.PadRight(paddedValue.Length + (4 - paddedValue.Length % 4) % 4, '=');
        return Convert.FromBase64String(paddedValue);
    }
}
