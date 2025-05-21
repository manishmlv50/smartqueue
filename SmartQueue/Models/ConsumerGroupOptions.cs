namespace SmartQueueDotNet.Models
{
    public class ConsumerGroupOptions<T>
    {
        public string Name { get; set; } = Guid.NewGuid().ToString();
        public bool ProcessInBatch { get; set; } = false;
        public int BatchSize { get; set; } = 10;
        public int MaxParallelism { get; set; } = 2;
        public TimeSpan DelayBetweenBatches { get; set; } = TimeSpan.Zero;
        public TimeSpan DelayBetweenItems { get; set; } = TimeSpan.Zero;
        public Func<List<T>, Task>? BatchProcessor { get; set; }
        public Func<T, Task>? SingleProcessor { get; set; }
    }
}