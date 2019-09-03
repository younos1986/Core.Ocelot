using Core.Ocelot.Configurations;
using Core.Ocelot.IPRateLimiters;
using Core.Ocelot.LoadBalancerFactories;
using Core.Ocelot.Middlewares.Authorization;
using Core.Ocelot.Middlewares.IPRateLimiting;
using Core.Ocelot.Servers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Core.Ocelot.OcelotMiddlewares
{
    public static class CoreOcelotMiddlewareExtensions
    {
        public static IApplicationBuilder UseCoreOcelot(this IApplicationBuilder app)
        {
            // System.Diagnostics.Debugger.Break();

            //app.Use(async (ctx, next) =>
            //{
            //    await next();
            //    if (ctx.Response.StatusCode == 204)
            //    {
            //        ctx.Response.ContentLength = 0;
            //    }
            //});


            app.UseMiddleware<AuthorizationMiddleware>();

            app.UseMiddleware<IPRateLimitingMiddleware>();

            app.UseMiddleware<LoadBalancerMiddleware>();

            return app;
        }

        public static IServiceCollection AddCoreOcelot(this IServiceCollection services , Action<CoreOcelotConfiguration> coreOcelotConfig = null)
        {
            //System.Diagnostics.Debugger.Break();

            services.AddScoped<ILoadBalancerFactory, LoadBalancerFactory>();
            services.AddScoped<IIpHasher, IpHasher>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IIPRateLimiter, IPRateLimiter>();
            services.AddScoped<IIpAddressParser, RemoteIpParser>();
            


            var coreOcelotConfiguration = new CoreOcelotConfiguration();
            coreOcelotConfig.Invoke(coreOcelotConfiguration);

            services.AddSingleton<CoreOcelotConfiguration>(provider  =>
            {
                return coreOcelotConfiguration;
            });

            var _configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            var ybmLoadBalancer = _configuration.GetSection("LoadBalancer").Get<LoadBalance>();
            foreach (var lb in ybmLoadBalancer?.Servers)
            {
                var serverIdentity = lb.IP + lb.Port;
                services.AddHttpClient(serverIdentity, c =>
                {
                    c.BaseAddress = new Uri(serverIdentity);
                });
            }

            return services;
        }
    }
}
