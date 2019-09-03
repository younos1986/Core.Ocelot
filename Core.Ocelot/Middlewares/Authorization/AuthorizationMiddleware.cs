using Core.Ocelot.Configurations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ocelot.Middlewares.Authorization
{

    public class AuthorizationMiddleware
    {
        private RequestDelegate nextMiddleware;
        public AuthorizationMiddleware(RequestDelegate next)
        {
            this.nextMiddleware = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //System.Diagnostics.Debugger.Break();

            if (context == null) return;

            CoreOcelotConfiguration coreOcelotConfiguration = (CoreOcelotConfiguration)context.RequestServices.GetService(typeof(CoreOcelotConfiguration));

            if (coreOcelotConfiguration != null && coreOcelotConfiguration.EnableAutorization)
            {
                coreOcelotConfiguration.CoreOcelotAuthorizer.Authorize(context);
            }

            await this.nextMiddleware.Invoke(context);
        }
    }
}
