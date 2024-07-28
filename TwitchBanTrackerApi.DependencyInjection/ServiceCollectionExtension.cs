using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TwitchBanTrackerApi.Common.Mappings;
using TwitchBanTrackerApi.Infrastructure.Persistence;
using TwitchBanTrackerApi.Services;

namespace TwitchBanTrackerApi.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
            services.AddDbContext<TwitchBanTrackerDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("TwitchBanTrackerDb")));
            services.AddControllers().AddNewtonsoftJson();
            services.AddLogging();
            services.AddHostedService<TwitchListenerService>();
            services.AddHttpClient<TwitchUserMessages>();
            services.AddAutoMapper(typeof(MappingProfile));
        }
    }
}