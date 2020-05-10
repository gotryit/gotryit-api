using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using gotryit_api.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

        public void ConfigureServices(IServiceCollection services)
        {     


        #if DEBUG
            var connectionString = Configuration["Postgres:Connection"];
            var tokenKey = Convert.FromBase64String(Convert.ToBase64String(new byte[32]));// Configuration["Jwt:Key"]);
        #else
            //Heroku       
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var connectionString = GetPostgresConnection(databaseUrl);
        #endif

            services.AddControllers();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });
            
            services.AddEntityFrameworkNpgsql().AddDbContext<GoTryItContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(new byte[32]),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddCors(options => {
                options.AddPolicy("allOrigins", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });
            
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseCors("allOrigins");

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1");
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
