using System.Collections.Concurrent;

namespace Core.Ocelot.Configurations
{
    public interface IIpHasher
    {
        IpHasher Get(string clientIP);
        void Add();
    }
}