using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Middlewares.IPRateLimiting
{
    public static class LoadBalancerMiddlewareExtension
    {
        public static IApplicationBuilder UseCoreLoadBalancer(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoadBalancerMiddleware>();
        }
    }
}
