# Core.Ocelot is an ASP.NET Core middleware to implement API Gateways in Microservice based applications


## Features

* LoadBalancing  (based on Nginx) (HealthCheck is not implemented yet)
* IpRateLimiting
* Authorization


## Usage 

In appsetting.json

```

"LoadBalancer": {
    "LoadBalancerAlgorithm": "LeastConnection", // "LeastConnection" "RoundRobin"  "DynamicRoundRobinConnection" "HealthCheck"
    "HealthCheckInterval": 5,
    "IpHash": false,
    "IpHasherExpirationHours": 12,
    "Servers": [
      {
        "Name": "ServerA",
        "IP": "https://localhost",
        "Port": "44360",
        "Weight": 1,
        "MaxFails": 3,
        "FailTimeout": 30
      },
      {
        "Name": "ServerB",
        "IP": "https://localhost",
        "Port": "44316",
        "Weight": 1,
        "MaxFails": 3,
        "FailTimeout": 30
      }
    ]
  },
  "IPRateLimitingSetting": {
    "EnableEndpointRateLimiting": true,
    "DisableRateLimitHeaders": false,
    "StatusCode": "429",
    "Message": "Too Many Requests",
    "IPWhitelist": [ "192.168.0.0/24" ], //  "127.0.0.1","::1/10"
    "IPBlockedlist": [ "192.168.5.10", "192.168.5.11" ],
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/values",
        "Period": "1s",
        "Limit": 2
      },
      {
        "Endpoint": "GET:/api/values/get",
        "Period": "60s",
        "Limit": 2
      },
      {
        "Endpoint": "*:/api/values2",
        "Period": "12h",
        "Limit": 1000
      },
      {
        "Endpoint": "*",
        "Period": "7d",
        "Limit": 10000
      }
    ]
  },

```




In Startup file 

```



public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // source code is in the sample project 
            AddAuthorization(services);


            services.AddScoped<CustomCoreOcelotAuthorizer, CustomCoreOcelotAuthorizer>();
            services.AddCoreOcelot(config =>
            {
                config.EnableAutorization = true;
                config.CoreOcelotAuthorizer = new CustomCoreOcelotAuthorizer(
                    Configuration,
                    services.BuildServiceProvider().GetService<IMemoryCache>()
                    );

                // in case to disable IPRateLimiting - no configuraion part as "IPRateLimitingSetting" is needed 
                // config.IPRateLimitingSetting = new IPRateLimitingSetting() { EnableEndpointRateLimiting = false };
                config.IPRateLimitingSetting = Configuration.GetSection("IPRateLimitingSetting").Get<IPRateLimitingSetting>();

            });
        }
        
```        


```

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            app.UseAuthentication();

            //ignore Ocelat ReRoutes
            AppMapWhen(app);

            app.UseCoreOcelot();


            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void AppMapWhen(IApplicationBuilder app)
        {
            List<string> ignoreList = new List<string>()
            {
                "/api/CustomValues",
            };

            //Add the specific route to app.MapWhen To ignore Ocelat route capturing.
            app.MapWhen((context) =>
            {
                return ignoreList.Any(q => context.Request.Path.StartsWithSegments(q));
            }, (appBuilder) =>
            {
                appBuilder.UseMvc();
            });
        }
        
```
