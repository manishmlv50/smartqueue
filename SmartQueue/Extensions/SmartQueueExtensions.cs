using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartQueueDotNet.Models;

namespace SmartQueueDotNet.Extensions
{
    public static class SmartQueueServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartQueue<T>(this IServiceCollection services, QueueOptions options)
        {
            services.AddSingleton(sp => new SmartQueue<T>(options));
            return services;
        }
    }
}