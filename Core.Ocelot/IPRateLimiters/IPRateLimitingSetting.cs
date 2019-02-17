using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.IPRateLimiters
{
    public class IPRateLimitingSetting
    {
        public bool EnableEndpointRateLimiting { get; set; }
        public bool DisableRateLimitHeaders { get; set; }
        public int StatusCode { get; set; } = 429;
        public string Message { get; set; } = "Too Many Requests";
        public List<string> IPWhitelist { get; set; }
        public List<string> IPBlockedlist { get; set; }
        public List<IPRateLimitingGeneralRule> GeneralRules { get; set; }
    }

    public class IPRateLimitingGeneralRule
    {
        public string Endpoint
        {
            get { return Verbs + Path; }
            set
            {
                var endpointParts = value.Split(':');

                if (endpointParts.Length == 1)
                {
                    Verbs = endpointParts[0];
                }
                else if (endpointParts.Length == 2)
                {
                    Verbs = endpointParts[0];
                    Path = endpointParts[1];
                }
            }
        }
        public string All { get; private set; }
        public string Path { get; private set; }
        public string Verbs { get; private set; }
        public int PeriodTime { get; private set; }
        public string Period
        {
            get { return PeriodTime.ToString(); }
            set
            {

                var period = value.ToString();
                if (period.EndsWith("s"))
                {
                    var time = period.Substring(0, period.IndexOf('s'));
                    PeriodTime = int.Parse(time);
                }
                else if (period.EndsWith("m"))
                {
                    var time = period.Substring(0, period.IndexOf('m'));
                    PeriodTime = int.Parse(time) * 60;
                }
                else if (period.EndsWith("h"))
                {
                    var time = period.Substring(0, period.IndexOf('h'));
                    PeriodTime = int.Parse(time) * 60 * 60;
                }
                
            }
        }
        public int Limit { get; set; }
    }
}
