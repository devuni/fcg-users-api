using System;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Users.Application.Interfaces;
using Users.Infrastructure.Messaging;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Security;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(opt =>
        {
            var cs = configuration.GetConnectionString("Default") ?? configuration["ConnectionStrings:Default"];
            opt.UseSqlite(cs);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        var enableRabbitMq = configuration.GetValue<bool>("Messaging:EnableRabbitMQ", true);

        if (enableRabbitMq)
        {
            services.AddScoped<IEventBus, MassTransitEventBus>();

            // Messaging (MassTransit 8.x)
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var host = configuration["RabbitMQ:Host"] ?? "rabbitmq";
                    cfg.Host(host, "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                });
            });
        }
        else
        {
            services.AddScoped<IEventBus, NoopEventBus>();
        }

        // JWT
        // Use Jwt:Secret consistently (token generation uses Jwt:Secret)
        var secret = configuration["Jwt:Secret"] ?? configuration["Jwt:Key"] ?? "super-secret-demo-key-change-me";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "fcg-fase2",
                    ValidAudience = configuration["Jwt:Audience"] ?? "fcg-fase2",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
