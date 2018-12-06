using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace WebApplication
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
            //string con = "Server=(localdb)\\mssqllocaldb;Database=usersdbstore;Trusted_Connection=True;MultipleActiveResultSets=true";
            services.AddDbContext<UsersContext>(options => options.UseSqlServer(Configuration["AppSettings:DefaultAzureDb"]));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc(options =>
                {
                    options.FormatterMappings.SetMediaTypeMappingForFormat
                        ("xml", MediaTypeHeaderValue.Parse("application/xml"));
                    options.FormatterMappings.SetMediaTypeMappingForFormat
                        ("config", MediaTypeHeaderValue.Parse("application/xml"));
                    options.FormatterMappings.SetMediaTypeMappingForFormat
                        ("json", MediaTypeHeaderValue.Parse("application/json"));
                }).AddXmlSerializerFormatters();
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
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
