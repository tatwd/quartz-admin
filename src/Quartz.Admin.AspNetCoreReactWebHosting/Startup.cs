using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz.Admin.AspNetCoreReactWebHosting.Data;

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
            services.AddTransient<HttpSendJob>();
            services.AddTransient<CoreService>();

            // default is ServiceLifetime.Scoped
            services.AddDbContext<JobStoreContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("MyJobStore"));
            });

            services.AddQuartz(q =>
            {
                // q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseMicrosoftDependencyInjectionScopedJobFactory();

                // required
                q.UseSimpleTypeLoader();

                // job store
                // q.UseInMemoryStore();
                // q.UsePersistentStore(c =>
                // {
                //     c.UseSQLite(Configuration.GetConnectionString("MyJobStore"));
                // });
            });

            services.AddQuartzServer(c =>
            {
                c.WaitForJobsToComplete = true;
            });

            services.AddControllersWithViews()
                .ConfigureApiBehaviorOptions(s =>
                {
                    s.SuppressModelStateInvalidFilter = true;
                    s.InvalidModelStateResponseFactory = context =>
                    {
                        // TODO: impl my invalid result
                        var result = new BadRequestResult();
                        return result;
                    };
                });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });

            services.AddHttpClient();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
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

            // app.UseStatusCodePages(builder => {
            //     builder.Run(async ctx => {
            //         if (ctx.Response.StatusCode == StatusCodes.Status500InternalServerError) {
            //             ctx.Response.ContentType = "application/json; charset=utf-8";
            //             await ctx.Response.WriteAsync("{\"code\":1500，\"message\": \"server error\"}");
            //         }
            //     });
            // });

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

            lifetime.ApplicationStarted.Register(OnAppStarted);
        }

        private void OnAppStarted()
        {
            // TODO: start auto jobs
            // Console.WriteLine("started");
        }
    }
}