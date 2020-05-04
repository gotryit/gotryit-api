using System;
using System.Text.RegularExpressions;
using gotryit_api.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace gotryit_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private string Get(string valueName, Match match)
        {
            return match.Groups[valueName].Value.ToString();
        }

        private string GetPostgresConnection(string databaseUrl)
        {
            var connectionDataExtractor = @"^postgres:\/\/(?<user>\w+):(?<password>\w+)@(?<server>\S+):(?<port>\d+)\/(?<database>\w+)$";

            var connectionData = Regex.Match(databaseUrl, connectionDataExtractor);

            var result = $"Server={Get("server", connectionData)};Port={Get("port", connectionData)};User Id={Get("user", connectionData)};Password={Get("password", connectionData)};Database={Get("database", connectionData)}";

            return result;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {     


        #if DEBUG
            var connectionString = Configuration["Postgres:Connection"];
        #else
            //Heroku       
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var connectionString = GetPostgresConnection(databaseUrl);
        #endif

            services.AddControllers();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather API", Version = "v1" });
            });
            
            services.AddEntityFrameworkNpgsql().AddDbContext<GoTryItContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
