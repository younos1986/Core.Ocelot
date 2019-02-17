using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Middlewares.IPRateLimiting
{
    public static class IPRateLimitingMiddlewareExtension
    {

        public static IApplicationBuilder UseCoreIPRateLimiting(this IApplicationBuilder app)
        {

            return app.UseMiddleware<IPRateLimitingMiddleware>();
        }

       

    }
}
