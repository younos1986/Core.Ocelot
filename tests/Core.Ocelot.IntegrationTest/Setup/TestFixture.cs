using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Core.Ocelot.IntegrationTest.Setup
{
    public class TestFixture : IDisposable
    {
        private readonly TestServer _mainApiServer;
        private readonly TestServer _serverAApiServer;
        private readonly TestServer _serverBApiServer;

        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

        }
        public TestFixture()
        {
            var projectDir = System.IO.Directory.GetCurrentDirectory();//.Replace(@"bin\Debug\netcoreapp2.2", "");
            var configurationRoot = GetIConfigurationRoot(projectDir);

             var mainBuilder = new WebHostBuilder()
                 .UseEnvironment("Development")
                 .UseContentRoot(projectDir)
                 .UseConfiguration(configurationRoot).UseStartup<MainServer.Startup>();
            _mainApiServer = new TestServer(mainBuilder);

            MainApiClient = _mainApiServer.CreateClient();
            MainApiClient.BaseAddress = new Uri("https://localhost:5000");


            //var serverABuilder = new WebHostBuilder()
            //     .UseEnvironment("Development")
            //     .UseContentRoot(projectDir)
            //     .UseStartup<ServerA.Startup>();
            //_serverAApiServer = new TestServer(serverABuilder);

            //ServerAApiClient = _serverAApiServer.CreateClient();
            //ServerAApiClient.BaseAddress = new Uri("http://localhost:5001");


            //var serverBBuilder = new WebHostBuilder()
            // .UseEnvironment("Development")
            // .UseContentRoot(projectDir)
            // .UseStartup<ServerB.Startup>();
            //_serverBApiServer = new TestServer(serverBBuilder);

            //ServerBApiClient = _serverBApiServer.CreateClient();
            //ServerBApiClient.BaseAddress = new Uri("http://localhost:5002");





        }

        protected HttpClient MainApiClient { get; }
        protected HttpClient ServerAApiClient { get; }
        protected HttpClient ServerBApiClient { get; }


        public void Dispose()
        {
            MainApiClient.Dispose();
            ServerAApiClient.Dispose();
            ServerBApiClient.Dispose();

            _mainApiServer.Dispose();
            _serverAApiServer.Dispose();
            _serverBApiServer.Dispose();

        }
    }
}
