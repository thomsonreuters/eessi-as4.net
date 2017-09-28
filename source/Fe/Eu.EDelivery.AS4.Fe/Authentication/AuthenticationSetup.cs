using System;
using System.Linq;
using System.Security.Claims;
using Eu.EDelivery.AS4.Fe.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    /// <summary>
    /// Setup authentication
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Authentication.IAuthenticationSetup" />
    public class AuthenticationSetup : IAuthenticationSetup
    {
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            RegisterOptions(services, configuration);

            var databaseSettings = configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();

            services.AddDbContext<ApplicationDbContext>(options => SqlConnectionBuilder.Build(databaseSettings.Provider, databaseSettings.ConnectionString, options));
            services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        public void Run(IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<JwtOptions>>().Value;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = options.SigningKey,
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = false,
                ValidAudience = options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role
            };

            app.Use(async (context, next) =>
            {
                if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                {
                    if (context.Request.QueryString.HasValue)
                    {
                        var token = context.Request.QueryString.Value
                            .Split('&')
                            .SingleOrDefault(x => x.Contains("access_token"))?.Split('=')[1];
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            context.Request.Headers.Add("Authorization", new[] { $"Bearer {token}" });
                        }
                    }
                }
                await next.Invoke();
            });

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = "JWT",
                TokenValidationParameters = tokenValidationParameters
            });
        }

        private static void RegisterOptions(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));
            services.Configure<JwtOptions>(configuration.GetSection("Authentication:JwtOptions"));
        }
    }
}