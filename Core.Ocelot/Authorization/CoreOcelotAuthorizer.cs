using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Ocelot.Configurations
{
    public class CoreOcelotAuthorizer : BaseCoreOcelotAuthorizer
    {
        public override bool Authorize(HttpContext context)
        {
            return true;

            bool hasAccess = false;

            var extractedPath = ExtractPath(context.Request.Path);
            if (!string.IsNullOrEmpty(extractedPath.controller) || !string.IsNullOrEmpty(extractedPath.action))
            {
                var currentClaimValue = $"{extractedPath.area}:{extractedPath.controller}:{extractedPath.action}";
                var hashedCurrentClaimValue = GetSha256Hash(currentClaimValue);

                var user = context.User;
                hasAccess = user.Claims.Any(q => q.Type == "DynamicPermission" && q.Value == hashedCurrentClaimValue);
            }

            //if (hasAccess == false)
            //    throw new UnauthorizedAccessException("You do not have sufficent access");
            //else
            //    return true;

            return  true;


            
        }
    }
}
