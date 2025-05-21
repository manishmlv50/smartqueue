# SmartQueue v1.0.9 Release Notes

## Overview

SmartQueue is a robust, configurable queue processing library for .NET 8 designed to handle complex scenarios involving multiple consumer groups, batch processing, CPU-based throttling, and metrics integration. It provides a flexible, leak-proof queue abstraction suitable for scenarios requiring concurrent processing and dynamic load management.

## Features

- Multi-consumer support where each consumer group processes all queued messages independently
- Batch and single-item processing modes with configurable batch sizes and delays
- CPU usage monitoring with throttling to prevent overload during high CPU utilization
- Parallelism control with semaphore throttling based on configured max concurrency
- Integration with Microsoft.Extensions.Logging for diagnostic logging
- Support for Metrics using System.Diagnostics.Metrics for enqueue and processed counts
- Dependency Injection support via IServiceCollection extensions for easy integration
- Graceful shutdown and queue draining support
- Demo console app showcasing real-world use case: User registration and onboarding workflows with multiple queues and consumer groups
- Well-structured codebase with separation of concerns and clear API design

## Use Cases

SmartQueue can be used in:

- Background task processing in microservices and API backends
- Event-driven processing pipelines with multiple subscribers
- Throttled batch processing of messages or jobs in scalable applications
- Integration with Azure Functions or worker services requiring in-memory or durable queue abstractions
- Systems that require dynamic CPU-aware throttling to maintain responsiveness
- Complex workflows requiring multiple consumer groups with different processing logic on the same queue

## Getting Started

See the included demo console app in the solution that implements a user registration and onboarding system with multiple consumer groups and queues. This demo illustrates how to:

- Enqueue user registration requests from multiple producers concurrently
- Process queued items in parallel with batch and single item processors
- Chain multiple queues to model dependent workflows (e.g., registration → onboarding completion)
- Log each processing step with consumer group tags

## Known Limitations and Future Work

- Metrics support is basic and can be extended to export to Prometheus, Application Insights, etc.
- Durable queue backing (e.g., Kafka, Azure Service Bus) not implemented but planned
- More advanced error handling and retry policies to be added

---

Thank you for trying SmartQueue! Feedback and contributions are welcome.