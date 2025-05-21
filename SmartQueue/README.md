# SmartQueue

SmartQueue is a lightweight and high-performance in-memory queueing library for .NET 8+, designed for local parallel task processing with flexible throttling, metrics, and consumer group support.

## Features

- Multiple consumer groups (each receives the message independently)
- Configurable batch or single message processing
- Built-in CPU load throttling to avoid overloading your system
- Retry and error handling built into consumers
- Efficient in-memory queue with Channel<T>
- Integrated queue metrics for visibility and diagnostics

## Installation

Install via NuGet:

```bash
 add package SmartQueue
```

## Core Concepts

### Queue Registration

You can register a SmartQueue for a specific type with custom options:

```csharp
services.AddSmartQueue<MyEvent>(new QueueOptions
{
    MaxQueueSize = 5000,
    DropMessagesWhenFull = false,
});
```

### Consumer Registration

Each SmartQueue can have one or more independent consumer groups:

```csharp
smartQueue.RegisterConsumer(new ConsumerGroupOptions<MyEvent>
{
    Name = "LoggerGroup",
    SingleProcessor = async item =>
    {
        Console.WriteLine($"Event received: {item.Name}");
        await Task.CompletedTask;
    }
});
```

## Demo Use Case: User Registration Workflow

The SmartQueueDemoApp console application simulates a user registration workflow using SmartQueue.

### Flow Overview

Queue 1: userRegistrationQueue

- New users are randomly added to the queue in parallel
- Consumers:
  1. Log "User Registered"
  2. Log "Onboarding Email Sent" → then enqueue to Queue 2
  3. Log "Learning Path Assigned"

Queue 2: onboardingQueue

- Triggered once onboarding email is sent
- Consumers:
  1. Log "Notification to Org Admin Sent"
  2. Log "Org Kit Sent"

### Logging Only

The demo logs each operation to simulate real-world async processing across services without implementing the actual features (emailing, DB write, etc.).

## Potential Use Cases

- Event broadcasting across multiple subsystems (e.g., audit + notification)
- CPU-aware in-process parallel workers
- Real-time pipeline chaining (e.g., ETL, ML inference)
- Queue mocking for integration testing

## Example Integration

```csharp
services.AddSmartQueue<MyPayload>();

var myQueue = serviceProvider.GetRequiredService<SmartQueue<MyPayload>>();

myQueue.RegisterConsumer(new ConsumerGroupOptions<MyPayload>
{
    Name = "MyConsumer",
    SingleProcessor = async item =>
    {
        Console.WriteLine($"Processed: {item}");
        await Task.Delay(100);
    }
});

await myQueue.EnqueueAsync(new MyPayload { Name = "Example" });
```

## License

MIT License  
© 2025 SmartQueue Contributors