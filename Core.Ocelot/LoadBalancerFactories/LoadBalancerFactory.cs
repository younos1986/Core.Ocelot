using Core.Ocelot.Configurations;
using Core.Ocelot.Enums;
using Core.Ocelot.Servers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Core.Ocelot.LoadBalancerFactories
{
    public class LoadBalancerFactory : ILoadBalancerFactory
    {

        IIpHasher ipHasher;
        IHttpContextAccessor httpContextAccessor;
        IHttpClientFactory httpClientFactory;
        public LoadBalancerFactory(
            IConfiguration _configuration,
            IIpHasher _ipHasher,
            IHttpContextAccessor _httpContextAccessor,
            IHttpClientFactory _httpClientFactory)
        {
            if (StaticFields.LoadBalancer == null)
                StaticFields.LoadBalancer = _configuration.GetSection("LoadBalancer").Get<LoadBalance>();

            ipHasher = _ipHasher;
            httpContextAccessor = _httpContextAccessor;
            httpClientFactory = _httpClientFactory;
        }


        public Server Get()
        {
            var bestIp = GetBestIP();

            if (StaticFields.LoadBalancer.IpHash == false)
                return bestIp;

            var clientIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            var usedIp = ipHasher.Get(clientIp);
            if (usedIp != null && usedIp.Server != null)
                return usedIp.Server;

            var ipHashObject = new IpHasher()
            {
                ClientIP = clientIp,
                DestinationIp = bestIp.IP + ":" + bestIp.Port,
                CreatedAt = DateTime.Now,
                Server = bestIp
            };
            ipHashObject.Add();

            return bestIp;
        }
        private Server GetBestIP()
        {
            
            if (!StaticFields.LoadBalancer.Servers.Any())
                throw new Exception("YbmLoadBalancer server part is not defined in appsettings.json");

            if (StaticFields.LoadBalancer.Servers.Any(q => q.WaitForFailTimeout))
                MakeCircuit();

            Server selectedServer = null;

            var algorithm = (EnumLoadBalancerAlgorithm)Enum.Parse(typeof(EnumLoadBalancerAlgorithm), StaticFields.LoadBalancer.LoadBalancerAlgorithm.ToString());
            switch (algorithm)
            {
                case EnumLoadBalancerAlgorithm.LeastConnection :    // leat connections count is first
                    selectedServer = LeastConnection();
                    break;

                case EnumLoadBalancerAlgorithm.RoundRobin:    // get server byn its weight, most is first
                    selectedServer = RoundRobinConnection();
                    break;

                case EnumLoadBalancerAlgorithm.DynamicRoundRobinConnection:   // get server by its weight, most is first. weight calculated dynamically
                    selectedServer = DynamicRoundRobinConnection();
                    break;

                //case EnumLoadBalancerAlgorithm.HealthCheck:  // get server by its health (available RAM), most available is first. health calculated every 5 seconds
                //    if (algorithm == EnumLoadBalancerAlgorithm.HealthCheck && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                //    {
                //        selectedServer = HealthCheck();
                //    }
                //    else
                //    if (algorithm == EnumLoadBalancerAlgorithm.HealthCheck && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                //    {
                //        throw new NotImplementedException("health_check algorithm is not supported in this platform yet");
                //    }
                //    else
                //    if (algorithm == EnumLoadBalancerAlgorithm.HealthCheck  && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                //    {
                //        throw new NotImplementedException("health_check algorithm is not supported in this platform yet");
                //    }
                //    break;

                default:
                    Console.WriteLine("Default case");
                    break;
            }
            return selectedServer;
        }

        public void IncreaseFailedCount(Server s)
        {
            s.TriedFails++;
            if (s.TriedFails >= s.MaxFails)
            {
                s.WaitForFailTimeout = true;
                s.LastFailedAt = DateTime.Now;
            }
        }

        private Server LeastConnection()
        {
            Server selectedServer;
            selectedServer = (from server in StaticFields.LoadBalancer.Servers
                              where server.WaitForFailTimeout == false
                              orderby server.FiredCount
                              select server).First();

            selectedServer.FiredCount++;
            return selectedServer;
        }

        private Server RoundRobinConnection()
        {
            Server selectedServer;
            //if (!StaticFields.YbmLoadBalancer.Servers.All(q => q.Weight > 0))
            //    throw new Exception("");

            foreach (var s in StaticFields.LoadBalancer.Servers)
            {
                if (s.FiredCount > 0)
                    s.WeightDividByFiredCount = (decimal)(s.Weight) / (decimal)s.FiredCount;
                else
                    s.WeightDividByFiredCount = (decimal)(s.Weight) / (decimal)1;    // when s.FiredCount is zero, it throws DivideByZeroException
            }

            selectedServer = (from server in StaticFields.LoadBalancer.Servers
                              where server.WaitForFailTimeout == false
                              orderby server.WeightDividByFiredCount descending
                              select server).First();

            selectedServer.FiredCount++;
            return selectedServer;
        }

        private Server DynamicRoundRobinConnection()
        {
            Server selectedServer = null;


            if (!StaticFields.LoadBalancer.Servers.All(q => q.FixedSizedQueues.Count > 1))
            {
                return LeastConnection();
            }

            foreach (var s in StaticFields.LoadBalancer.Servers)
            {
                if (s.FiredCount > 0)
                    s.WeightDividByFiredCount = (decimal)(s.FixedSizedQueues.Sum()) / (decimal)s.FiredCount;
                else
                    s.WeightDividByFiredCount = (decimal)(s.FixedSizedQueues.Sum()) / (decimal)1;    // when s.FiredCount is zero, it throws DivideByZeroException
            }


            selectedServer = (from server in StaticFields.LoadBalancer.Servers
                              orderby server.WeightDividByFiredCount descending
                              select server).First();

            selectedServer.FiredCount++;
            return selectedServer;
        }

        private Server HealthCheck()
        {
            Server selectedServer;

            StartdHealthCkeckerThread();

            if (StaticFields.LoadBalancer.Servers.Any(q => q.UsageObject == null))
            {
                return LeastConnection();
            }



            foreach (var s in StaticFields.LoadBalancer.Servers)
            {
                if (s.FiredCount > 0)
                    s.AvailableRamDividByFiredCount = (decimal)(s.UsageObject.AvailableRam) / (decimal)s.FiredCount;
                else
                    s.AvailableRamDividByFiredCount = (decimal)(s.UsageObject.AvailableRam) / (decimal)1;    // when s.FiredCount is zero, it throws DivideByZeroException
            }


            selectedServer = (from server in StaticFields.LoadBalancer.Servers
                              where server.UsageObject.IsHealthy
                              orderby server.UsageObject.AvailableRam ascending
                              select server).First();

            selectedServer.FiredCount++;
            return selectedServer;
        }

        static bool threadIsRunning = false;
        static object lockObject = new object();
        private void StartdHealthCkeckerThread()
        {
            if (StaticFields.LoadBalancer.HealthCheckInterval < 5)
                throw new Exception("HealthCheckInterval cannot be less than 5");


            lock (lockObject)
            {
                if (threadIsRunning == true)
                    return;

                threadIsRunning = true;
            }

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    foreach (var server in StaticFields.LoadBalancer.Servers)
                    {
                        try
                        {
                            var ip = server.IP + ":" + server.Port;// + "/ResourceUsageCheck";
                            var httpClient = httpClientFactory.CreateClient(ip);
                            var response = httpClient.GetAsync(ip + "/ResourceUsageCheck").GetAwaiter().GetResult();
                            response.EnsureSuccessStatusCode();
                            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var usageObject = JsonConvert.DeserializeObject<UsageObject>(content);
                            server.UsageObject = usageObject;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(StaticFields.LoadBalancer.HealthCheckInterval));
                }

            });
        }

        private static void MakeCircuit()
        {
            foreach (var s in StaticFields.LoadBalancer.Servers)
            {
                if (DateTime.Now > s.LastFailedAt.AddSeconds(s.FailTimeout))
                {
                    s.WaitForFailTimeout = false;
                    s.TriedFails = 0;
                    s.FiredCount = 0;
                }
            }
        }
    }
}
