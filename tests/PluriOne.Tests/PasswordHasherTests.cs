using PluriOne.AuthService.Services;
using Xunit;

namespace PluriOne.Tests;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_And_Verify_ReturnsTrue()
    {
        var hash = _hasher.Hash("PluriOne#2026");
        Assert.True(_hasher.Verify("PluriOne#2026", hash));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("PluriOne#2026");
        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_IsSalted_ProducesDifferentHashes()
    {
        var h1 = _hasher.Hash("PluriOne#2026");
        var h2 = _hasher.Hash("PluriOne#2026");
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void Hash_DoesNotContainPlainPassword()
    {
        var hash = _hasher.Hash("PluriOne#2026");
        Assert.DoesNotContain("PluriOne#2026", hash);
    }

    [Fact]
    public void Hash_UsesArgon2id()
    {
        var hash = _hasher.Hash("PluriOne#2026");
        Assert.StartsWith("$argon2id$", hash);
    }

    [Fact]
    public void RefreshToken_HashToken_IsDeterministic()
    {
        var a = PasswordHasher.HashToken("token-123");
        var b = PasswordHasher.HashToken("token-123");
        Assert.Equal(a, b);
        Assert.NotEqual(a, PasswordHasher.HashToken("token-456"));
    }
}
