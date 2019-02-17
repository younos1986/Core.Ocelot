using Core.Ocelot.Servers;

namespace Core.Ocelot.LoadBalancerFactories
{
    public interface ILoadBalancerFactory
    {
        Server Get();

        void IncreaseFailedCount(Server s);
    }
}