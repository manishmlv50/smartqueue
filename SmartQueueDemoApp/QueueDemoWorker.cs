using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartQueueDotNet.Models;
using SmartQueueDotNet;
using Microsoft.Extensions.DependencyInjection;

namespace SmartQueueDemoApp
{
    public class QueueDemoWorker : BackgroundService
    {
        private readonly SmartQueue<UserModel> _registrationQueue;
        private readonly SmartQueue<UserModel> _onboardingQueue;
        private readonly ILogger<QueueDemoWorker> _logger;

        public QueueDemoWorker(IServiceProvider serviceProvider, ILogger<QueueDemoWorker> logger)
        {
            _registrationQueue = serviceProvider.GetRequiredService<SmartQueue<UserModel>>();
            _onboardingQueue = serviceProvider.GetServices<SmartQueue<UserModel>>().Last(); // Second instance
            _logger = logger;

            RegisterConsumers();
        }

        private void RegisterConsumers()
        {
            // Consumer 1 - Log user registration
            _registrationQueue.RegisterConsumer(new ConsumerGroupOptions<UserModel>
            {
                Name = "UserRegistrationLogger",
                SingleProcessor = async user =>
                {
                    await Task.Delay(100); // Simulate work
                    _logger.LogInformation($"[Registered] User: {user.Id} - {user.Email}");
                }
            });

            // Consumer 2 - Log onboarding email + enqueue to onboardingQueue
            _registrationQueue.RegisterConsumer(new ConsumerGroupOptions<UserModel>
            {
                Name = "OnboardingEmail",
                SingleProcessor = async user =>
                {
                    await Task.Delay(150); // Simulate email work
                    _logger.LogInformation($"[Email] Onboarding email sent to: {user.Email}");
                    await _onboardingQueue.EnqueueAsync(user);
                }
            });

            // Consumer 3 - Assign learning path
            _registrationQueue.RegisterConsumer(new ConsumerGroupOptions<UserModel>
            {
                Name = "LearningPath",
                SingleProcessor = async user =>
                {
                    await Task.Delay(80);
                    _logger.LogInformation($"[Learning] Assigned learning path to: {user.Email}");
                }
            });

            // Onboarding Consumer
            _onboardingQueue.RegisterConsumer(new ConsumerGroupOptions<UserModel>
            {
                Name = "OnboardingHandler",
                SingleProcessor = async user =>
                {
                    await Task.Delay(100);
                    _logger.LogInformation($"[Notify] Sent notification to org admin for: {user.Email}");
                    _logger.LogInformation($"[Kit] Org Kit sent for: {user.Email}");
                }
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Simulate random users being added every 500ms
            var rnd = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                var user = new UserModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = $"user{rnd.Next(1000, 9999)}@demo.com"
                };

                await _registrationQueue.EnqueueAsync(user);
                _logger.LogInformation($"[Enqueue] New user added: {user.Email}");

                await Task.Delay(500, stoppingToken);
            }
        }
    }
}
