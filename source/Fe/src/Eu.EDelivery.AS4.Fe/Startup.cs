using System.IO;
using System.Net;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Runtime;
using Eu.EDelivery.AS4.Fe.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Fe
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class Startup
    {
        public IConfigurationRoot Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            var moduleMappings = services.BuildServiceProvider().GetService<IOptions<ApplicationSettings>>().Value.Modules;
            IConfigurationRoot config;
            services.AddModules(moduleMappings, (configBuilder, env) =>
            {
                configBuilder
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);
            }, out config);
            Configuration = config;

            // Add framework services.
            //services.AddAuthentication(options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddMvc().AddJsonOptions(options => { options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; });
            services.AddSingleton<ILogging, Logging.Logging>();
            services.AddSingleton<ISettingsSource, FileSettingsSource>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IRuntimeLoader, RuntimeLoader>();

            services.AddOptions();
            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));
            services.Configure<ApplicationSettings>(Configuration.GetSection("Settings"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline		Path.Combine(Directory.GetCurrentDirectory(), @"ui/dist/")	"C:\\dev\\AS4.net\\source\\Fe\\src\\Eu.EDelivery.AS4.Fe\\bin\\Debug\\ui/dist/"	string

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.ExecuteStartupServices();
            app.UseStatusCodePagesWithReExecute("/");
            app.UseDefaultFiles();
            app.UseStaticFiles();

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

            var logger = app.ApplicationServices.GetService<ILogging>();
            var settings = app.ApplicationServices.GetService<IOptions<ApplicationSettings>>();
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
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

            app.UseMvc();
        }
    }
}