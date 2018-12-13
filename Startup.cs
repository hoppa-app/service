using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

using hoppa.Service.Core;
using hoppa.Service.Data;
using hoppa.Service.Interfaces;
using hoppa.Service.Model;

namespace hoppa.Service
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
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddOData();
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddAzureAdBearer(options =>
                Configuration.Bind("AzureAd", options));
            services.AddMvc();
            
            var section = Configuration.GetSection("hoppa");
            services.Configure<Configuration>(section);

            services.AddTransient<IPersonRepository, PersonRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<Configuration> settings)
        {
            if (!(File.Exists(@"bunq.conf")))
            {
                Intergrations.bunq.Connection.Register(env, settings.Value.Service.Intergrations.bunq.apiKey);
            }
            Intergrations.bunq.Connection.Initialize();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseCors("CorsPolicy");
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(b =>
            {
                b.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                b.MapODataServiceRoute("regular", "api/v1.0", GetEntitySets());
                b.EnableDependencyInjection();
            });
        }

        private static IEdmModel GetEntitySets()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Person>("People"); //Work in progress
            builder.Singleton<Person>("Person"); //Work in progress
            builder.EntitySet<Account>("Accounts"); //Work in progress
            builder.EntitySet<Connection>("Connections"); //Work in progress
            builder.EntitySet<Rule>("Rules"); //Work in progress
    
            return builder.GetEdmModel();
        }
    }
}
