using Core.Ocelot.Configurations;
using Core.Ocelot.IPRateLimiters;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ocelot.Middlewares.IPRateLimiting
{
    public class IPRateLimitingMiddleware
    {

        private RequestDelegate nextMiddleware;
        public IPRateLimitingMiddleware(RequestDelegate next)
        {
            this.nextMiddleware = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //System.Diagnostics.Debugger.Break();
            if (context == null) return;

            CoreOcelotConfiguration coreOcelotConfiguration = (CoreOcelotConfiguration)context.RequestServices.GetService(typeof(CoreOcelotConfiguration));
            IIPRateLimiter ipRateLimiter = (IIPRateLimiter)context.RequestServices.GetService(typeof(IIPRateLimiter));

            if (coreOcelotConfiguration != null && coreOcelotConfiguration.IPRateLimitingSetting.EnableEndpointRateLimiting == false)
            {
                await this.nextMiddleware.Invoke(context);
                return;
            }

            if (coreOcelotConfiguration.IPRateLimitingSetting == null)
                throw new Exception("EnableEndpointRateLimiting is true but no configuration is provided");

            var _options = coreOcelotConfiguration.IPRateLimitingSetting;
            var isExceeded = ipRateLimiter.RateExceeded(context, _options, out int retryAfter);

            if (isExceeded == false)
            {
                await this.nextMiddleware.Invoke(context);
                return;
            }

            if (!_options.DisableRateLimitHeaders)
            {
                context.Response.Headers["Retry-After"] = $@"Retry after {retryAfter} second(s)"; ;
            }

            context.Response.StatusCode = coreOcelotConfiguration.IPRateLimitingSetting.StatusCode;
            await context.Response.WriteAsync(coreOcelotConfiguration.IPRateLimitingSetting.Message);
            return;
        }
    }
}
