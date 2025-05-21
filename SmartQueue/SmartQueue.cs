
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SmartQueueDotNet.Models;
using System.Diagnostics.Metrics;

namespace SmartQueueDotNet
{
    public class SmartQueue<T>
    {
        private readonly Dictionary<string, Channel<QueueMessage<T>>> _consumerChannels = new();
        private readonly List<ConsumerGroupOptions<T>> _consumers = new();
        private readonly QueueOptions _options;
        private readonly CancellationTokenSource _cts = new();
        private readonly ILoggerFactory? _loggerFactory;
        private readonly ILogger? _logger;
        private readonly Meter _meter = new("SmartQueue.Metrics", "1.0.0");
        private readonly Counter<long> _enqueuedCounter;
        private readonly Counter<long> _processedCounter;
        private volatile bool _throttleDueToCpu = false;

        public SmartQueue(QueueOptions options, ILoggerFactory? loggerFactory = null)
        {
            _options = options;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger("SmartQueue");
            _enqueuedCounter = _meter.CreateCounter<long>("queue.enqueued", unit: "messages");
            _processedCounter = _meter.CreateCounter<long>("queue.processed", unit: "messages");
            _ = Task.Run(() => MonitorCpuUsage(_cts.Token));
        }

        public void RegisterConsumer(ConsumerGroupOptions<T> options)
        {
            var logger = _loggerFactory?.CreateLogger($"SmartQueue.Consumer.{options.Name}");
            var channel = Channel.CreateBounded<QueueMessage<T>>(_options.MaxQueueSize);
            _consumerChannels[options.Name] = channel;
            _consumers.Add(options);
            _ = Task.Run(() => StartConsumerLoop(options, channel, _cts.Token, logger));
        }

        public async Task EnqueueAsync(T item)
        {
            if (_throttleDueToCpu)
            {
                _logger?.LogWarning("Queue throttled due to high CPU load.");
                return;
            }

            var msg = new QueueMessage<T> { Payload = item };
            foreach (var kv in _consumerChannels)
            {
                await kv.Value.Writer.WriteAsync(msg);
            }

            _enqueuedCounter.Add(1);
        }

        private async Task StartConsumerLoop(ConsumerGroupOptions<T> options, Channel<QueueMessage<T>> channel, CancellationToken token, ILogger? logger)
        {
            var buffer = new List<T>();
            var semaphore = new SemaphoreSlim(options.MaxParallelism);

            while (!token.IsCancellationRequested)
            {
                if (channel.Reader.TryRead(out var msg))
                {
                    buffer.Add(msg.Payload);
                    if (!options.ProcessInBatch || buffer.Count >= options.BatchSize)
                    {
                        var toProcess = new List<T>(buffer);
                        buffer.Clear();

                        await semaphore.WaitAsync(token);
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                if (options.ProcessInBatch && options.BatchProcessor != null)
                                {
                                    await options.BatchProcessor(toProcess);
                                    _processedCounter.Add(toProcess.Count);
                                }
                                else if (options.SingleProcessor != null)
                                {
                                    foreach (var item in toProcess)
                                    {
                                        await options.SingleProcessor(item);
                                        _processedCounter.Add(1);
                                        if (options.DelayBetweenItems > TimeSpan.Zero)
                                            await Task.Delay(options.DelayBetweenItems);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger?.LogError(ex, "Error processing messages in {ConsumerName}", options.Name);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, token);

                        if (options.DelayBetweenBatches > TimeSpan.Zero)
                            await Task.Delay(options.DelayBetweenBatches, token);
                    }
                }
                else
                {
                    await Task.Delay(10, token);
                }
            }
        }

        private async Task MonitorCpuUsage(CancellationToken token)
        {
            var proc = Process.GetCurrentProcess();
            var lastCpuTime = proc.TotalProcessorTime;
            var lastCheck = DateTime.UtcNow;

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_options.CpuCheckIntervalMs, token);

                var currentCpuTime = proc.TotalProcessorTime;
                var currentTime = DateTime.UtcNow;

                var cpuUsedMs = (currentCpuTime - lastCpuTime).TotalMilliseconds;
                var totalTimeMs = (currentTime - lastCheck).TotalMilliseconds * Environment.ProcessorCount;
                var cpuUsagePercent = (cpuUsedMs / totalTimeMs) * 100;

                _throttleDueToCpu = cpuUsagePercent >= _options.CpuThreshold;
                _logger?.LogInformation("CPU Usage: {CpuUsage:F2}% (Throttle: {Throttle})", cpuUsagePercent, _throttleDueToCpu);

                lastCpuTime = currentCpuTime;
                lastCheck = currentTime;
            }
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            foreach (var channel in _consumerChannels.Values)
            {
                channel.Writer.Complete();
                while (await channel.Reader.WaitToReadAsync())
                {
                    channel.Reader.TryRead(out _);
                }
            }
        }

    }

}
