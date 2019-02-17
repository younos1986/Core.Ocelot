using Core.Ocelot.Servers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Configurations
{
    public static class StaticFields
    {
        public static ConcurrentDictionary<string, IpHasher> IpHasherPairs = new ConcurrentDictionary<string, IpHasher>();

        public static LoadBalance LoadBalancer { get;  set; }

    }
}
