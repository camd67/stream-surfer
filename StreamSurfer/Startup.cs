using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StreamSurfer.Models;
using StreamSurfer.Models.ShowViewModels;
using StreamSurfer.Services;

namespace StreamSurfer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.Configure<Settings>(Configuration);

            services.AddDbContext<PostgresDataContext>(options => options.UseNpgsql(
                Configuration["DbConn"],
                b => b.MigrationsAssembly("StreamSurfer")
                )
            );

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<PostgresDataContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // TODO: Update these settings so they're more strict
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                
                options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
                options.Cookies.ApplicationCookie.LoginPath = "/Account/LogIn";
                options.Cookies.ApplicationCookie.LogoutPath = "/Account/LogOff";
                
                options.User.RequireUniqueEmail = true;
            });

            services.AddSingleton<IWebRequestHandler, HttpClientRequestHandler>();
            services.AddTransient<IMessageService, FileMessageService>();
            services.AddScoped<IShowService, GuideboxService>();
            int cacheSize = int.Parse(Configuration.GetSection("SearchCacheSize").Value);
            services.AddSingleton(new RotatingCache<List<Show>>(cacheSize));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
            }

            app.UseIdentity();
            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AutomaticChallenge = true,
                AutomaticAuthenticate = true,
                CookieHttpOnly = true,
                // Force secure cookies in production
                CookieSecure = env.IsDevelopment() 
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
