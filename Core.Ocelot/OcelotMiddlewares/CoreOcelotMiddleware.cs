using Core.Ocelot.Configurations;
using Core.Ocelot.Extensions;
using Core.Ocelot.LoadBalancerFactories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Core.Ocelot.OcelotMiddlewares
{
    public class CoreOcelotMiddleware
    {
        private RequestDelegate nextMiddleware;
        public CoreOcelotMiddleware(RequestDelegate next)
        {
            this.nextMiddleware = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains("favicon.ico"))
                await this.nextMiddleware.Invoke(context);
            else
                await InvokeRequest(context);
        }

        private async Task InvokeRequest(HttpContext context)
        {
           // System.Diagnostics.Debugger.Break();


            await this.nextMiddleware.Invoke(context);


        }
       
       

       
    }
}
