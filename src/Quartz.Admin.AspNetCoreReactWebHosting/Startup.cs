using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Quartz.Admin.AspNetCoreReactWebHosting
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
            services.AddQuartz(q =>
            {
                // required
                q.UseSimpleTypeLoader();

                // var jobKey = new JobKey("awesome job", "awesome group");
                // q.AddJob<HttpSendJob>(c => c
                //     .WithIdentity(jobKey)
                //     .WithDescription("my awesome job"));
                // q.AddTrigger(t => t
                //     .WithIdentity("Simple Trigger")
                //     .ForJob(jobKey)
                //     .StartNow()
                //     .WithSimpleSchedule(x => x.
                //         WithInterval(TimeSpan.FromSeconds(10))
                //         .RepeatForever())
                //     .WithDescription("my awesome simple trigger")
                // );

                // job store
                q.UseInMemoryStore();
                // q.UsePersistentStore(s =>
                // {
                //     s.UseJsonSerializer();
                //     s.UseSQLite(o =>
                //     {
                //         o.ConnectionString = "Data Source=JobStore.db";
                //     });
                // });
            });

            services.AddQuartzServer(c =>
            {
                c.WaitForJobsToComplete = true;
            });

            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}