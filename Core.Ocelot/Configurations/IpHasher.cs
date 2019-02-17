using Core.Ocelot.Servers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Configurations
{
    public class IpHasher : IIpHasher
    {
        
        public string ClientIP { get; set; }
        public string DestinationIp { get; set; }
        public DateTime CreatedAt { get; set; }
        public Server Server { get; set; }

        public IpHasher Get(string clientIP)
        {
            if (StaticFields.LoadBalancer.IpHasherExpirationHours <= 0)
                throw new Exception("IpHasherExpirationHours cannot be lessthan 1");

            IpHasher existedIpHasher;
            if (!StaticFields.IpHasherPairs.TryGetValue(clientIP, out existedIpHasher))
                return null;

            if (DateTime.Now > existedIpHasher.CreatedAt.AddHours(StaticFields.LoadBalancer.IpHasherExpirationHours))
            {
                StaticFields.IpHasherPairs.TryRemove(ClientIP, out existedIpHasher);
                return null;
            }

            return existedIpHasher;
        }

        public void Add()
        {
            IpHasher existedIpHasher;
            if (StaticFields.IpHasherPairs.ContainsKey(ClientIP))
            {
                if (StaticFields.IpHasherPairs.TryGetValue(ClientIP, out existedIpHasher))
                {
                    if (DateTime.Now > existedIpHasher.CreatedAt.AddHours(StaticFields.LoadBalancer.IpHasherExpirationHours))
                    {
                        StaticFields.IpHasherPairs.TryRemove(ClientIP, out existedIpHasher);
                    }
                }
            }

            var obj = new IpHasher()
            {
                ClientIP = ClientIP,
                DestinationIp = DestinationIp,
                CreatedAt = DateTime.Now,
                Server = Server
            };
            StaticFields.IpHasherPairs.AddOrUpdate(ClientIP, obj, (key , oldValue) => obj);
        }

       
    }
}
