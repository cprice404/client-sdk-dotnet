using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedMegaCache;

public static class MegaCacheSessionExtensions
{
    public static IServiceCollection AddDistributedMegaCache(
        this IServiceCollection services,
        Action<MegaCacheConfiguration> configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }
        services.Configure(configure);
        return services.AddSingleton<IDistributedCache, MegaCacheDistributedCache>();
    }
}