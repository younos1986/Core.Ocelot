using Microsoft.AspNetCore.Http;

namespace Core.Ocelot.IPRateLimiters
{
    public interface IIPRateLimiter
    {
        bool RateExceeded(HttpContext context, IPRateLimitingSetting iPRateLimitingSetting, out int retryAfter);
    }
}