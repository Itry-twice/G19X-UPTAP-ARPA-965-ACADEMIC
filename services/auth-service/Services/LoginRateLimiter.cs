using StackExchange.Redis;

namespace PluriOne.AuthService.Services;

/// <summary>
/// Protección contra fuerza bruta y rate limiting usando Redis.
/// </summary>
public sealed class LoginRateLimiter
{
    private readonly IDatabase _db;

    private const int MaxFailuresPerAccount = 5;
    private const int MaxFailuresPerIp = 20;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    public LoginRateLimiter(IConnectionMultiplexer multiplexer)
    {
        _db = multiplexer.GetDatabase();
    }

    public async Task<bool> IsLockedAsync(string email, string ip)
    {
        var accountLock = await _db.KeyExistsAsync(LockKey(email));
        var ipLock = await _db.KeyExistsAsync(LockKey($"ip:{ip}"));
        return accountLock || ipLock;
    }

    public async Task RegisterFailureAsync(string email, string ip)
    {
        var accountKey = FailKey(email);
        var ipKey = FailKey($"ip:{ip}");

        var accountCount = await _db.StringIncrementAsync(accountKey);
        await _db.KeyExpireAsync(accountKey, Window);

        var ipCount = await _db.StringIncrementAsync(ipKey);
        await _db.KeyExpireAsync(ipKey, Window);

        if (accountCount >= MaxFailuresPerAccount)
        {
            await _db.StringSetAsync(LockKey(email), "1", LockDuration);
        }

        if (ipCount >= MaxFailuresPerIp)
        {
            await _db.StringSetAsync(LockKey($"ip:{ip}"), "1", LockDuration);
        }
    }

    public async Task ResetAsync(string email, string ip)
    {
        await _db.KeyDeleteAsync(FailKey(email));
        await _db.KeyDeleteAsync(FailKey($"ip:{ip}"));
    }

    private static string FailKey(string id) => $"login:fail:{id}";
    private static string LockKey(string id) => $"login:lock:{id}";
}
