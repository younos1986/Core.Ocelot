using Core.Ocelot.Configurations;
using Core.Ocelot.Extensions;
using Core.Ocelot.LoadBalancerFactories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Ocelot.Middlewares.IPRateLimiting
{
    public class LoadBalancerMiddleware
    {
        private RequestDelegate nextMiddleware;
        public LoadBalancerMiddleware(RequestDelegate next)
        {
            this.nextMiddleware = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // System.Diagnostics.Debugger.Break();

            ILoadBalancerFactory loadBalancerFactory = (ILoadBalancerFactory)context.RequestServices.GetService(typeof(ILoadBalancerFactory));
            IHttpClientFactory httpClientFactory = (IHttpClientFactory)context.RequestServices.GetService(typeof(IHttpClientFactory));

            var server = loadBalancerFactory.Get();

            var ip = string.Empty;
            if (string.IsNullOrWhiteSpace(server.Port))
                ip = server.IP;
            else
                ip = server.IP + ":" + server.Port;

            var path = context.Request.Path;


            var httpClient = httpClientFactory.CreateClient(ip);
            HttpResponseMessage response = null;
            try
            {
                ThreadLocal<Stopwatch> _localStopwatch = new ThreadLocal<Stopwatch>(() => new Stopwatch());
                _localStopwatch.Value.Reset();
                _localStopwatch.Value.Start();

                response = await ExecuteRequest(context, ip, path, httpClient, response);




                _localStopwatch.Value.Stop();
                server.FixedSizedQueues.Enqueue(_localStopwatch.Value.ElapsedMilliseconds);
                return;
            }
            catch (HttpRequestException ex)
            {
                loadBalancerFactory.IncreaseFailedCount(server);

                response.EnsureSuccessStatusCode();
                await PrepareResponse(context, response);
            }


            await this.nextMiddleware.Invoke(context);
        }

        private static async Task<HttpResponseMessage> ExecuteRequest(HttpContext context, string ip, PathString path, HttpClient httpClient, HttpResponseMessage response)
        {
            string url = MakeUrl(context, ip, path);

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(context.Request.Method), url);
            request.Content = await context.Request.MapContent();

            httpClient.AddAuthorizationHeaderIfExistsOnRequest(context);

            response = httpClient.SendAsync(request).GetAwaiter().GetResult();


            response.EnsureSuccessStatusCode();
            await PrepareResponse(context, response);


            return response;
        }

        private static string MakeUrl(HttpContext context, string ip, PathString path)
        {
            var queryParams = ConvertQueryStringToDictionary(context.Request.QueryString.ToString());
            var url = QueryHelpers.AddQueryString(ip + path, queryParams);
            return url;
        }

        private static Dictionary<string, string> ConvertQueryStringToDictionary(string queryString)
        {
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers
                .ParseQuery(queryString);

            Dictionary<string, string> temp = new Dictionary<string, string>();
            foreach (var q in query)
            {
                temp.Add(q.Key, q.Value.ToString());
            }
            return temp;
        }

        private static async Task PrepareResponse(HttpContext context, HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentLength != null)
            {
                AddHeaderIfDoesntExist(context, new Header("Content-Length", new[] { response.Content.Headers.ContentLength.ToString() }));
            }

            var content = await response.Content.ReadAsStreamAsync();

            using (content)
            {
                if (response.StatusCode != HttpStatusCode.NotModified && context.Response.ContentLength != 0)
                {
                    await content.CopyToAsync(context.Response.Body);
                }
            }

            context.Response.StatusCode = (int)response.StatusCode;
            if (context.Response.StatusCode == 204)
            {
                context.Response.ContentLength = 0;
            }

            //context.Response.ContentType = new MediaTypeHeaderValue("application/json").MediaType;
        }

        private static void AddHeaderIfDoesntExist(HttpContext context, Header httpResponseHeader)
        {
            if (!context.Response.Headers.ContainsKey(httpResponseHeader.Key))
            {
                context.Response.Headers.Add(httpResponseHeader.Key, new StringValues(httpResponseHeader.Values.ToArray()));
            }
        }

        private async Task<byte[]> ToByteArray(Stream stream)
        {
            using (stream)
            {
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    return memStream.ToArray();
                }
            }
        }

    }
}
