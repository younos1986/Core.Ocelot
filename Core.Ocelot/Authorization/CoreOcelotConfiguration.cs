using Core.Ocelot.IPRateLimiters;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Ocelot.Configurations
{
    public class CoreOcelotConfiguration
    {

        public bool EnableAutorization  { get; set; }
        public BaseCoreOcelotAuthorizer CoreOcelotAuthorizer { get; set; }


        public IPRateLimitingSetting IPRateLimitingSetting { get; set; }
        public Func<HttpContext, Func<Task>, Task> AuthenticationMiddlewareProp { get; set; }



    }
}
