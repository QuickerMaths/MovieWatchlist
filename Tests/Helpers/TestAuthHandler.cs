using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tests.Helpers;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemaName = "Test";
    public const string TestUserId = "test-user-1";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, TestUserId) };
        var identity = new ClaimsIdentity(claims, SchemaName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemaName);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}