// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using CalendarEvents.IDP.Data;
using CalendarEvents.IDP.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;

namespace CalendarEvents.IDP
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            string clientUri = Environment.GetEnvironmentVariable("ClientUri") ?? "localhost:4200";//todo cha host.docker.internal

            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      //TODO: Use the config instead.
                                      builder
                                      .AllowAnyOrigin()
                                      //.SetIsOriginAllowed(origin => origin == $"http://{clientUri}")
                                      .AllowAnyHeader()
                                      .AllowAnyMethod();
                                  });
            });

            //// configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            //services.Configure<IISOptions>(iis =>
            //{
            //    iis.AuthenticationDisplayName = "Windows";
            //    iis.AutomaticAuthentication = false;
            //});

            //// configures IIS in-proc settings
            //services.Configure<IISServerOptions>(iis =>
            //{
            //    iis.AuthenticationDisplayName = "Windows";
            //    iis.AutomaticAuthentication = false;
            //});

            string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                string server = Environment.GetEnvironmentVariable("DatabaseServer") ?? "localhost";
                string database = Environment.GetEnvironmentVariable("DatabaseName") ?? "CalendarEventsIDPDB";
                string port = Environment.GetEnvironmentVariable("DatabasePort") ?? "1443";
                string user = Environment.GetEnvironmentVariable("DatabaseUser") ?? "sa";
                string password = Environment.GetEnvironmentVariable("DatabasePassword") ?? "<YourStrong@Passw0rd>";
                connectionString = $"Server={server},{port};Database={database};User ID={user};Password={password};";
            }
            else
            {
                Console.WriteLine("==================Azure connection string=======================");
                Console.WriteLine(connectionString);
                Console.WriteLine("==================Azure connection string=======================");
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()                
                .AddDefaultTokenProviders();
            
            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.IssuerUri = Environment.GetEnvironmentVariable("IssuerUri") ?? "http://idp";
                })
                .AddInMemoryIdentityResources(Config.Ids)
                .AddInMemoryApiResources(Config.Apis)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();


            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to http://localhost:5000/signin-google
                    options.ClientId = "copy client ID from Google here";
                    options.ClientSecret = "copy client secret from Google here";
                });

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
            | SecurityProtocolType.Tls11
            | SecurityProtocolType.Tls12;            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            PrepareDB.PreparePopulation(app).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();                
            }

            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Strict });
            app.UseStaticFiles();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            }
        }
    }
}