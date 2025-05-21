using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartQueueDemoApp;
using SmartQueueDotNet;
using SmartQueueDotNet.Extensions;
using SmartQueueDotNet.Models;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });

        // Register queues with default options
        services.AddSmartQueue<UserModel>(new QueueOptions
        {
            MaxQueueSize = 10000,
            CpuThreshold = 85,
            DropOnFullQueue = false,
            AutoDrainOnShutdown = true,
            CpuCheckIntervalMs = 3000
        });
        // Second queue for onboarding
        services.AddSmartQueue<UserModel>(new QueueOptions
        {
            MaxQueueSize = 10000,
            CpuThreshold = 85,
            DropOnFullQueue = false,
            AutoDrainOnShutdown = true,
            CpuCheckIntervalMs = 3000
        });

        services.AddHostedService<QueueDemoWorker>();
    })
    .Build();

await host.RunAsync();
