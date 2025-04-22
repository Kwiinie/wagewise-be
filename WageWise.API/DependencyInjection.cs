using WageWise.Application.Interfaces.Services;
using WageWise.Domain.Interfaces;
using WageWise.Infrastructure;
using WageWise.Infrastructure.Services;

namespace WageWise.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAIService, AIService>();
            services.AddScoped<ICVService, CVServices>();
            services.AddScoped<IStorageService, StorageService>();

            return services;
        }
    }
}
