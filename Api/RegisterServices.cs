using Api.Data;
using Api.Framework;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FluentValidation;

namespace Api
{
    public static class RegisterServices
    {
        public static void AddApiServices(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("myDb"));

            // Assembly Scanners
            services.AddServices<Program>()
                    .AddValidation<Program>();

            services.AddAuthentication("bearer")
                    .AddJwtBearer("bearer", opt =>
                    {
                        opt.Events = new();
                        opt.Events.OnMessageReceived = (ctx) =>
                        {
                            // TODO - Custom Handling of the JWT goes here. Read the Database to verify the User/Token reading some Header passed in

                            // Hack: Authentication is mocked here! This is simply hardcoding and mocking up the token for now!
                            var claims = new Claim[] { new("username", "bob master 3000") };
                            var identity = new ClaimsIdentity(claims, "bearer");
                            ctx.Principal = new ClaimsPrincipal(identity);
                            ctx.Success();
                            return Task.CompletedTask;
                        };
                    });

            services.AddAuthorization(config => 
            {
                config.AddPolicy("admin", policyBuilder => policyBuilder.RequireAuthenticatedUser()
                                                                        .AddAuthenticationSchemes("bearer"));
            });
        }

        public static IServiceCollection AddServices<T>(this IServiceCollection services)
        {
            var targetServices = typeof(T).Assembly.GetTypes()
                .Select(t => (t, t.BaseType))
                .Where((tuple) => tuple.BaseType != null)
                .Where((tuple) => tuple.BaseType.IsGenericType 
                               && tuple.BaseType.GetGenericTypeDefinition().IsEquivalentTo(typeof(Handler<,>)));

            foreach (var s in targetServices)
            {
                services.AddTransient(s.Item2, s.Item1);
            }

            return services;
        }

        public static IServiceCollection AddValidation<T>(this IServiceCollection services)
        {
            var validators = typeof(T).Assembly.GetTypes()
                .Select(t => (t, t.BaseType))
                .Where((tuple) => tuple.BaseType != null)
                .Where((tuple) => tuple.BaseType.IsGenericType
                               && tuple.BaseType.IsAbstract
                               && tuple.BaseType.GetGenericTypeDefinition().IsEquivalentTo(typeof(AbstractValidator<>)))
                .Select((tuple) => (tuple.t, tuple.BaseType.GetGenericArguments()[0]));

            foreach (var v in validators)
            {
                var validorInterfaceType = typeof(IValidator<>).MakeGenericType(v.Item2);
                services.AddTransient(validorInterfaceType, v.Item1);
            }

            return services;
        }
    }
}
