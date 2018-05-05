﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace NickDarvey.SampleApplication.ReliableService.AspNetCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) =>
            services.AddMvc();

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) =>
            app.UseMvc();
    }
}
