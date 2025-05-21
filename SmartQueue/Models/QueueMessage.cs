namespace SmartQueueDotNet.Models
{
    public class QueueMessage<T>
    {
        public T Payload { get; set; }
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    }
}