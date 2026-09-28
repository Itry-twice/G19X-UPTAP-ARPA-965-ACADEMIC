using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using PluriOne.Shared.Auth;
using Xunit;

namespace PluriOne.Tests;

public sealed class JwtTokenServiceTests
{
    private static JwtTokenService CreateService()
    {
        var options = Options.Create(new JwtOptions
        {
            SigningKey = "test-signing-key-that-is-long-enough-for-hmac-256",
            Issuer = "plurione-auth",
            Audience = "plurione",
            AccessMinutes = 30,
            RefreshDays = 7
        });
        return new JwtTokenService(options);
    }

    [Fact]
    public void CreateAccessToken_ContainsExpectedClaims()
    {
        var svc = CreateService();
        var user = new CurrentUser(Guid.NewGuid(), "a@example.com", "A", "colaborador");
        var token = svc.CreateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("plurione-auth", jwt.Issuer);
        Assert.Contains(jwt.Audiences, a => a == "plurione");
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal("colaborador", jwt.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public void ValidationParameters_ValidateIssuerAndAudience()
    {
        var svc = CreateService();
        var parameters = svc.ValidationParameters();
        Assert.True(parameters.ValidateIssuer);
        Assert.True(parameters.ValidateAudience);
        Assert.True(parameters.ValidateIssuerSigningKey);
    }

    [Fact]
    public void CurrentUser_IsStaff()
    {
        Assert.True(new CurrentUser(Guid.NewGuid(), "a@x.com", "A", "rrhh").IsStaff);
        Assert.True(new CurrentUser(Guid.NewGuid(), "a@x.com", "A", "admin").IsStaff);
        Assert.False(new CurrentUser(Guid.NewGuid(), "a@x.com", "A", "colaborador").IsStaff);
    }
}
