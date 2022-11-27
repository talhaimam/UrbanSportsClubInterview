using InterviewService.Jobs;
using InterviewService.Jobs.FitogramMQConsumers;
using Hangfire;
using Hangfire.Console;
using Hangfire.Pro.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using System;
using System.Text;
using StatsdClient;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace InterviewService
{
    public class Startup
    {
        private readonly Configuration Configuration;

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration.Get<Configuration>();

            #region Datadog
            StatsdClient.DogStatsd.Configure(new StatsdConfig
            {
                StatsdServerName = "172.17.0.1",
                Prefix = "InterviewService"
            });
            #endregion Datadog

            Serilog.Log.Information("Time zone info: " + TimeZoneInfo.Local.DisplayName);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            #region Add CORS [Required]
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetPreflightMaxAge(new System.TimeSpan(1, 0, 0, 0));
                    });
            });
            #endregion Add CORS [Required]

            #region Add Mvc [Required]
            services
              .AddMvcCore()
              .AddAuthorization()
              .AddNewtonsoftJson(opt => opt.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter()));

            services.AddMvc();
            #endregion Add Mvc [Required]

            #region Add JsonSerializerOptions

            System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            services.AddSingleton<System.Text.Json.JsonSerializerOptions>(jsonSerializerOptions);

            #endregion Add JsonSerializerOptions

            #region Add JWT Authentication [Required]
            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                       ValidateIssuerSigningKey = true,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(Configuration.JWT_TOKEN)),
                        ValidateLifetime = false
                    };
                });
            #endregion Add JWT Authentication [Required]

            #region AUTHORIZATION
            services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes("Bearer")
                        .Build();

                    options.AddPolicy("Admin", policy =>
                            policy
                            .AddAuthenticationSchemes("Bearer")
                            .RequireClaim("user_type", "admin"));

                    options.AddPolicy("Pro", policy =>
                            policy
                            .AddAuthenticationSchemes("Bearer")
                            .RequireClaim("user_type", "admin", "pro"));

                    options.AddPolicy("User", policy =>
                            policy
                            .AddAuthenticationSchemes("Bearer")
                            .RequireClaim("user_type", "admin", "pro", "user"));
                });
            #endregion AUTHORIZATION

            #region Add Swagger [Required]
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = $"{this.Configuration.SERVICENAME}",
                    Description = $"{this.Configuration.SERVICENAME}",
                    Version = "v1",
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                options.CustomSchemaIds(type =>
                {
                    return type.Name.EndsWith("DTO")
                        ? type.Name.Replace("DTO", "")
                        : type.Name;
                });

                var filePath = Path.Combine(AppContext.BaseDirectory, $"{nameof(InterviewService)}.xml");
                options.IncludeXmlComments(filePath);

                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/898#issuecomment-538474500
                options.DescribeAllParametersInCamelCase();
            });

            services.AddSwaggerGenNewtonsoftSupport();
            #endregion Add Swagger [Required]

            #region Add Configuration [Required]
            services.AddTransient<IStartupFilter, Helpers.ConfigurationValidationStartupFilter>();
            services.AddSingleton<Configuration>(this.Configuration);
            services.AddSingleton<Helpers.IValidator>(this.Configuration);
            #endregion Add Configuration [Required]

            #region Add Redis Persist key
            var redis = ConnectionMultiplexer.Connect($"{Configuration.HANGFIRE_REDIS_ENDPOINT}");
            services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            #endregion Add Redis Persist key

            #region Add DBContext [Optional]
            services.AddDbContext<DbContext>(options =>
                options
                    .UseLazyLoadingProxies(true)
                    .UseNpgsql(this.Configuration.DB_CONNECTION_STRING)
            );
            #endregion Add DBContext [Optional]

            #region Add Hangfire [Optional]
            services.AddHangfire(config =>
            {
                config.UseRedisStorage(this.Configuration.HANGFIRE_REDIS_ENDPOINT, new RedisStorageOptions { Prefix = this.Configuration.SERVICENAME });
                config.UseConsole();
                config.UseBatches();
            });
            #endregion Add Hangfire [Optional]

            #region Add FitogramMQ Client
            if (this.Configuration.RABBITMQ_DISABLE == false)
            {
                services.AddSingleton<FitogramMQ.IFitogramMQClient>(_ => new FitogramMQ.FitogramMQClient
                (
                    clientName: Configuration.SERVICENAME,
                    hostname: Configuration.RABBITMQ_ENDPOINT,
                    port: Convert.ToInt32(Configuration.RABBITMQ_PORT),
                    username: Configuration.RABBITMQ_USERNAME,
                    password: Configuration.RABBITMQ_PASSWORD
                ));
            }
            #endregion Add FitogramMQ Client

            #region Health Check

            services
                .AddHealthChecks()
                .AddNpgSql(name: "postgres", npgsqlConnectionString: this.Configuration.DB_CONNECTION_STRING)
                .AddRedis(name: "redis", redisConnectionString: this.Configuration.HANGFIRE_REDIS_ENDPOINT)
                .AddRabbitMQ(name: "rabbitmq", rabbitConnectionString: "amqp://"
                    + Configuration.RABBITMQ_USERNAME + ":"
                    + Configuration.RABBITMQ_PASSWORD + "@"
                    + Configuration.RABBITMQ_ENDPOINT + ":"
                    + Configuration.RABBITMQ_PORT);
            #endregion Health Check
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            ILogger<Startup> logger = services.GetRequiredService<ILogger<Startup>>();

            app.UseRouting();
            app.UseCors("AllowAllOrigins");

            #region Setup Error Handling [Required]
            if (env.IsDevelopment() || Configuration.ENVIRONMENT?.ToUpper() == "DEVELOPMENT")
            {
                //When we are in debug or developer mode, we show a detailed error page
                app.UseDeveloperExceptionPage();
            }
            #endregion Setup Error Handling [Required]

            #region Setup Swagger [Required]
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, _) =>
                {
                    // Otherwise on production it says the URL is "http://{service}.fitogram.pro" (notice the wrong schema).
                    // Swagger will then fail to make API requests, with error "Blocked loading mixed active content".
                    swagger.Servers = new List<OpenApiServer>();
                });
            });
            app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/swagger/v1/swagger.json", this.Configuration.SERVICENAME + " API"));
            #endregion Setup Swagger [Required]

            #region Run EF Migrations [Optional]
            {
                var database = services.GetRequiredService<DbContext>().Database;

                database.SetCommandTimeout(1800);

                logger.LogDebug("Running database migrations.");
                database.Migrate();
                logger.LogDebug("Finished running database migrations.");
            }
            #endregion Run EF Migrations [Optional]

            #region Setup Hangfire [Optional]
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                //The authorization is done by Cloudflare
                Authorization = new[] { new DisableAuthorization() }
            });

            if (this.Configuration.RUN_HANGFIRE_JOBS)
            {
                app.UseHangfireServer(new BackgroundJobServerOptions
                {
                    WorkerCount = 5,
                    Queues = new[] { Constants.Queues.Default, Constants.Queues.DefaultLow }
                });

                app.UseHangfireServer(new BackgroundJobServerOptions
                {
                    WorkerCount = 5,
                    Queues = new[] { Constants.Queues.Sync, }
                });

                app.UseHangfireServer(new BackgroundJobServerOptions
                {
                    WorkerCount = 1,
                    Queues = new[] { Constants.Queues.SyncLow, }
                });
            }

            //Enqueue recurring jobs
            if (Configuration.RUN_RECURRING_JOBS)
            {
            }
            #endregion Setup Hangfire [Optional]

            #region Add FitogramMQ Consumers/Publishers [Optional]
            if (this.Configuration.RABBITMQ_DISABLE == false)
            {
                var fitogramMQClient = services.GetService<FitogramMQ.IFitogramMQClient>();
                ProviderConsumer.Start(fitogramMQClient);
                EventConsumer.Start(fitogramMQClient);
                RoleConsumer.Start(fitogramMQClient);
                CustomerConsumer.Start(fitogramMQClient);
            }
            #endregion Add FitogramMQ Consumers/Publishers [Optional]

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapDefaultControllerRoute();
            });

            #region Health Check
            app
               .UseHealthChecks("/healthcheck", new HealthCheckOptions
               {
                   Predicate = _ => true,
                   ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
               })
               .UseHealthChecksUI(setup =>
               {
                   setup.UIPath = "/hc"; // this is ui path in your browser
                    setup.ApiPath = "/hc-api"; // the UI ( spa app )  use this path to get information from the store ( this is NOT the healthz path, is internal ui api )
                });
            #endregion Health Check
        }
    }
}