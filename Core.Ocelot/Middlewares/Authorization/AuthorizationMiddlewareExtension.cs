using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Middlewares.Authorization
{

    public static class AuthorizationMiddlewareExtension
    {

        public static IApplicationBuilder UseCoreAuthorization(this IApplicationBuilder app)
        {

            return app.UseMiddleware<AuthorizationMiddleware>();
        }


    }
}
