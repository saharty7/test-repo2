using System.Collections.Concurrent;

namespace HotelBookingPlatform.Common;

public class StaffTokenInfo
{
    public required string Username { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}

public class StaffTokenStore
{
    private readonly ConcurrentDictionary<string, StaffTokenInfo> _tokens = new();

    public string IssueToken(string username, TimeSpan lifetime)
    {
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        _tokens[token] = new StaffTokenInfo { Username = username, ExpiresAtUtc = DateTime.UtcNow.Add(lifetime) };
        return token;
    }

    public bool TryGetUsername(string token, out string username)
    {
        username = string.Empty;

        if (!_tokens.TryGetValue(token, out var info))
        {
            return false;
        }

        if (info.ExpiresAtUtc < DateTime.UtcNow)
        {
            _tokens.TryRemove(token, out _);
            return false;
        }

        username = info.Username;
        return true;
    }
}
