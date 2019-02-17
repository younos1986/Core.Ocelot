using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Ocelot.Configurations
{
    public abstract class BaseCoreOcelotAuthorizer
    {
        public abstract bool Authorize(HttpContext context);
        
        public (string area, string controller, string action) ExtractPath(string path)
        {
            var area = string.Empty;
            var controller = string.Empty;
            var action = string.Empty;

            if (path.ToLower().StartsWith("api"))
                return (area, controller, action);

            var pathParts = path.Split('/');

            ///api/area/Ticket/GetTicketBaseInfo?sth=11
            if (Regex.IsMatch(path, @"(\/api\/)(\w+\/)(\w+\/)(\w+)"))
            {
                area = pathParts[1];
                controller = pathParts[2];
                action = pathParts[3];
            }
            else
            ///api/Attachment/UploadAttachment
            if (Regex.IsMatch(path, @"(\/api\/)(\w+\/)(\w+)"))
            {
                controller = pathParts[2];
                action = pathParts[3];

            }
            return (area, controller, action);
        }

        public static string GetSha256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

    }
}
