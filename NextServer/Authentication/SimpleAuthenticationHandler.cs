namespace NextServer.Authentication
{
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class SimpleAuthenticationHandler : AuthenticationHandler<SimpleAuthenticationHandlerOptions>
    {
        public SimpleAuthenticationHandler(
            IOptionsMonitor<SimpleAuthenticationHandlerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (this.Request.Headers.TryGetValue("Authorization", out var authValues))
            {
                var subject = authValues.FirstOrDefault();

                if (!string.IsNullOrEmpty(subject))
                {
                    var claims = new[]
                    {
                        new Claim("sub", subject),
                        new Claim(ClaimTypes.NameIdentifier, subject),
                    };

                var identity = new ClaimsIdentity(claims, this.Scheme.Name);
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}
