using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Ocelot.IPRateLimiters
{
    public class IPRateCounter
    {
        public DateTime CreatedAt { get; set; }
        public int Count { get; set; }
    }
    public class IPRateLimiter : IIPRateLimiter
    {
        private static ConcurrentDictionary<string, IPRateCounter> IpRateLimiterDictionary = new ConcurrentDictionary<string, IPRateCounter>();
        IIpAddressParser _ipParser;
        public IPRateLimiter(IIpAddressParser ipAddressParser)
        {
            _ipParser = ipAddressParser;
        }

        public bool RateExceeded(HttpContext context, IPRateLimitingSetting setting, out int retryAfter)
        {
            retryAfter = 0;
            var path = context.Request.Path;
            var method = context.Request.Method.ToLower();

            var clientIP = _ipParser.GetClientIp(context);

            //System.Diagnostics.Debugger.Break();

            if (_ipParser.ContainsIp(setting.IPWhitelist, clientIP.ToString()))
                return false;

            if (_ipParser.ContainsIp(setting.IPBlockedlist, clientIP.ToString()))
                return true;

            var generalRuleExists = setting.GeneralRules.FirstOrDefault(q => q.Verbs.ToLower().Contains(method) && q.Path == path);
            if (generalRuleExists == null)
                return false;

            if (!IpRateLimiterDictionary.TryGetValue(clientIP.ToString(), out IPRateCounter existedRate))
            {
                var iPRateCounter = new IPRateCounter() { Count = 1, CreatedAt = DateTime.Now };
                IpRateLimiterDictionary.AddOrUpdate(clientIP.ToString(), iPRateCounter, (key, oldValue) => iPRateCounter);
                return false;
            }


            var now = DateTime.Now.AddSeconds(-generalRuleExists.PeriodTime);
            var createdAt = existedRate.CreatedAt;
            if (createdAt > now)
            {
                retryAfter = (int)(now - createdAt).TotalSeconds;
                if (existedRate.Count >= generalRuleExists.Limit)
                    return true;
                else
                {
                    existedRate.Count++;
                    return false;
                }
            }
            else
            {
                existedRate.Count = 1;
                existedRate.CreatedAt = DateTime.Now;
                IpRateLimiterDictionary.AddOrUpdate(clientIP.ToString(), existedRate, (key, oldValue) => existedRate);
                return false;
            }
        }
    }
}
