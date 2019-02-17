using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Servers
{
    public class Server
    {
        public Server()
        {
            FixedSizedQueues = new FixedSizedQueue<double>(100);
        }
        public string Name { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public long Weight { get; set; } = 1;
        public int MaxFails { get; set; }
        public int TriedFails { get; set; }
        public int FailTimeout { get; set; }
        public DateTime LastFailedAt { get; set; }
        public bool WaitForFailTimeout { get; set; }
        public int FiredCount { get; set; }

        public FixedSizedQueue<double> FixedSizedQueues { get; set; }
        public decimal AverageResponseTimeByFireCount { get; set; }
        public decimal WeightDividByFiredCount { get; set; }
        public decimal AvailableRamDividByFiredCount { get; set; }

        public UsageObject UsageObject { get; set; }

    }

    public class LoadBalance
    {
        public string LoadBalancerAlgorithm { get; set; }
        public int HealthCheckInterval { get; set; }
        public bool IpHash { get; set; }
        public int IpHasherExpirationHours { get; set; }
        public ICollection<Server> Servers { get; set; }
    }
}
