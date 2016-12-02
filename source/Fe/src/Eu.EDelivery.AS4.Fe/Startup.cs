using System.IO;
using System.Net;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Runtime;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Fe.Start;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog.Extensions.Logging;

namespace Eu.EDelivery.AS4.Fe
{
    // Add profile data for application users by adding properties to the ApplicationUser class

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);
            //if (env.IsEnvironment("Development"))
            //    builder.AddApplicationInsightsSettings(true);

            //builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddAuthentication(options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();
            services.AddSwaggerGen();
            services.AddAutoMapper();
            services.AddSingleton<ILogging, Logging.Logging>();
            services.AddSingleton<ISettingsSource, FileSettingsSource>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IRuntimeLoader, RuntimeLoader>(x => (RuntimeLoader) new RuntimeLoader(Path.Combine(Directory.GetCurrentDirectory(), "runtime")).Initialize());

            services.AddOptions();
            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));
            services.Configure<ApplicationSettings>(Configuration.GetSection("Settings"));

            // Setup Identity
            var connectionStringBuilder = new SqliteConnectionStringBuilder {DataSource = "test.sqlite"};
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var moduleMappings = services.BuildServiceProvider().GetService<IOptions<ApplicationSettings>>().Value.Modules;
            services.AddModules(moduleMappings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddNLog();

            app.SetupAuthentication();
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    ExpireTimeSpan = TimeSpan.FromSeconds(5)
            //});
            //var facebookOptions = new FacebookOptions
            //{
            //    AppId = "1827549694196393",
            //    AppSecret = "e8d815e06e16fed295b5889265e6f543",
            //    AuthenticationScheme = FacebookDefaults.AuthenticationScheme,
            //    //SignInScheme = "Test",
            //    AutomaticAuthenticate = false,
            //    AutomaticChallenge = false,
            //    BackchannelHttpHandler = new HttpClientHandler(),
            //    CallbackPath = "/api/authentication/externallogincallback?provider=Facebook",
            //    Events = new OAuthEvents
            //    {
            //        OnTicketReceived = ctx =>
            //        {
            //            var options = new JwtOptions();

            //            var jwt = new JwtSecurityToken(
            //                options.Issuer,
            //                options.Audience,
            //                null,
            //                options.NotBefore,
            //                options.Expiration,
            //                options.SigningCredentials);

            //            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            //            ctx.Response.Redirect("http://localhost:3000/#/login?token=" + encodedJwt);
            //            return Task.FromResult(0);
            //        }
            //    }
            //};

            env.ConfigureNLog("nlog.config");

            var logger = app.ApplicationServices.GetService<ILogging>();
            var settings = app.ApplicationServices.GetService<IOptions<ApplicationSettings>>();
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        var response = new
                        {
                            IsError = true,
                            Exception = !settings.Value.ShowStackTraceInExceptions ? null : ex.Error.StackTrace
                        };
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                        logger.Error(ex.Error);
                    }
                });
            });

            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                context.SaveChanges();

                var db = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                db.CreateAsync(new ApplicationUser {UserName = "test"}, "k1342hT*98").Wait();
            }

            app.ExecuteStartupServices();
        }
    }
}