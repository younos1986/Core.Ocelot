# Core.Ocelot



```

Ins Startup file 

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
