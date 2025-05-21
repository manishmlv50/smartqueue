namespace SmartQueueDotNet.Models
{
    public class QueueOptions
    {
        public int MaxQueueSize { get; set; } = 10000;
        public int CpuThreshold { get; set; } = 75;
        public bool DropOnFullQueue { get; set; } = false;
        public bool AutoDrainOnShutdown { get; set; } = true;
        public int CpuCheckIntervalMs { get; set; } = 2000;
    }
}