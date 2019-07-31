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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Services;
using TicTacToe.Extensions;

namespace TicTacToe
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IUserService, UserService>();
            services.AddRouting();
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
