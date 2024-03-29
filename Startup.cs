﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Services;
using TicTacToe.Extensions;
using System.Globalization;
using TicTacToe.Options;

namespace TicTacToe
{
    public class Startup
    {
        public IConfiguration _configuration { get; }
        public IHostingEnvironment _hostingEnvironment { get; }
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }
        public void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Localization");
            services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix,
                options => options.ResourcesPath = "Localization")
                .AddDataAnnotationsLocalization();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IGameInvitationService, GameInvitationService>();
            services.Configure<EmailServiceOptions>(_configuration.GetSection("Email"));
            services.AddEmailService(_hostingEnvironment, _configuration);
            services.AddRouting();
            services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(30);
            });
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }
        public void ConfigureStagingServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }
        public void ConfigureProductionServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            var options = new RewriteOptions()
             .AddRewrite("NewUser", "/UserRegistration/Index", false);
            app.UseRewriter(options);

            app.UseStaticFiles();
            app.UseSession();

            var routeBuilder = new RouteBuilder(app);
            routeBuilder.MapGet("CreateUser", context =>
             {
                 var firstName = context.Request.Query["firstName"];
                 var lastName = context.Request.Query["lastName"];
                 var email = context.Request.Query["email"];
                 var password = context.Request.Query["password"];
                 var userService = context.RequestServices.GetService<IUserService>();
                 userService.RegisterUser(new Models.UserModel
                 {
                     FirstName = firstName,
                     LastName = lastName,
                     Email = email,
                     Password = password
                 });
                 return context.Response.WriteAsync($"User {firstName} {lastName} has beend successfully created");
             });
            var newUserRoutes = routeBuilder.Build();
            app.UseRouter(newUserRoutes);

            app.UseWebSockets();
            app.UseCommunicationMiddleware();
            app.UseHttpsRedirection();

            var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-GB"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            localizationOptions.RequestCultureProviders.Clear();
            localizationOptions.RequestCultureProviders.Add(new CultureProviderResolverService());

            app.UseRequestLocalization(localizationOptions);

            app.UseCookiePolicy();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseStatusCodePages("text/plain", "HTTP ERROR - response code: {0}");
        }
    }
}
