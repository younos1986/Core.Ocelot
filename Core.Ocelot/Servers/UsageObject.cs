using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Servers
{
    public class UsageObject
    {
        public bool IsHealthy { get; set; }
        public int AvailableRam { get; set; }
        public int AvailableCpu { get; set; }

    }
}
