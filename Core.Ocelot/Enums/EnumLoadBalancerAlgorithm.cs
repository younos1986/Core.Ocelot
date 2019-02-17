using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Enums
{
    public enum EnumLoadBalancerAlgorithm
    {
        LeastConnection,
        RoundRobin,
        DynamicRoundRobinConnection,
        HealthCheck
    }
}
