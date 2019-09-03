using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ocelot.Extensions
{
    public static class HttpRequestExtensions
    {

        public async static Task<HttpContent> MapContent(this HttpRequest request)
        {
            if (request.Body == null || (request.Body.CanSeek && request.Body.Length <= 0))
            {
                return null;
            }

            // Never change this to StreamContent again, I forgot it doesnt work in #464.
            var content = new ByteArrayContent(await ToByteArray(request.Body));

            

            if (!string.IsNullOrEmpty(request.ContentType))
            {
                content.Headers
                    .TryAddWithoutValidation("Content-Type", new[] { request.ContentType });
            }

            AddHeaderIfExistsOnRequest("Content-Language", content, request);
            AddHeaderIfExistsOnRequest("Content-Location", content, request);
            AddHeaderIfExistsOnRequest("Content-Range", content, request);
            AddHeaderIfExistsOnRequest("Content-MD5", content, request);
            AddHeaderIfExistsOnRequest("Content-Disposition", content, request);
            AddHeaderIfExistsOnRequest("Content-Encoding", content, request);

            AddHeaderIfExistsOnRequest("Accept", content, request);
            AddHeaderIfExistsOnRequest("Accept-Encoding", content, request);
            AddHeaderIfExistsOnRequest("Accept-Language", content, request);
            AddHeaderIfExistsOnRequest("Access-Control-Allow-Origin", content, request);
            AddHeaderIfExistsOnRequest("Access-Control-Request-Method", content, request);
            AddHeaderIfExistsOnRequest("Access-Control-Request-Headers", content, request);


            // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "username", "password"))));

            return content;
        }

        private static void AddHeaderIfExistsOnRequest(string key, HttpContent content, HttpRequest request)
        {
            if (request.Headers.ContainsKey(key))
            {
                content.Headers
                    .TryAddWithoutValidation(key, request.Headers[key].ToList());
            }
        }
        public static void AddAuthorizationHeaderIfExistsOnRequest(this HttpClient httpClient, HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization",  context.Request.Headers["Authorization"].ToList() );
            }
        }
        private async static Task<byte[]> ToByteArray(Stream stream)
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
