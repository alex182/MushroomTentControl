﻿using System;
using System.Collections.Generic;
using System.Device.Gpio;
using HydroPiApi.BackgroundJobs;
using HydroPiApi.BackgroundJobs.JobStateHelper;
using HydroPiApi.BackgroundJobs.Models;
using HydroPiApi.Controllers.Common;
using HydroPiApi.Controllers.Common.Processor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RelayClient;
using RelayClient.Models;
using SensorClient;
using SensorClient.Models;
using SensorClient.SensorReadings;


namespace HydroPiApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var devMachineNames = Configuration.GetSection("DevMachineNames").Get<List<string>>();

            var relays = Configuration.GetSection("Relays").Get<List<Relay>>();
            var relayOptions = new RelayClientOptions
            {
                Relays = relays
            };

            var sensors = Configuration.GetSection("Sensors").Get<List<Sensor>>();
            var sensorOptions = new SensorClientOptions
            {
                Sensors = sensors
            };

            var humidifierJobOptions = Configuration.GetSection("HumidifierJobOptions")
                .Get<HumidifierPressureAltitudeTemperatureJobOptions>();

            var fanJobOptions = Configuration.GetSection("FanJobOptions")
                .Get<FanJobOptions>();

            JobStateHelper.AddOrUpdateJobState(new JobState() { JobOptions = humidifierJobOptions}, 
                "HumidifierPressureAltitudeTemperatureJob");

            JobStateHelper.AddOrUpdateJobState(new JobState() { JobOptions = fanJobOptions },
                "FanJob");

            //TODO: use a real logger eventually
            var loggerFactory = new LoggerFactory();

            services.AddTransient(provider => loggerFactory);

            if (devMachineNames.Contains(Environment.MachineName))
            {
                services.AddSingleton<IGpioController, MockGpioDriver>();
            }
            else
            {
                services.AddSingleton<IGpioController, GpioController>();
            }

            services.AddSingleton<HumidifierPressureAltitudeTemperatureJobOptions>(provider => humidifierJobOptions);
            services.AddSingleton<FanJobOptions>(provider => fanJobOptions);
            services.AddSingleton<IRelayClientOptions>(provider => relayOptions);
            services.AddSingleton<ISensorClientOptions>(provider => sensorOptions);
            services.AddTransient<ISensorReadingClientFactory, SensorReadingClientFactory>(); 
            services.AddSingleton<IRelayClient, RelayClient.RelayClient>();
            services.AddSingleton<ISensorClient, SensorClient.SensorClient>();
           
            services.AddTransient<IProcessorFactory, ProcessorFactory>();

            services.AddSingleton<IHostedService, HumidifierPressureAltitudeTemperatureJob>();
            services.AddSingleton<IHostedService, FanJob>();


            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }         

            app.UseMvc();
        }
    }
}
