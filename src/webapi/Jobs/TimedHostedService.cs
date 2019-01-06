namespace WebApi.Jobs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cronos;
    using Microsoft.Extensions.Hosting;

    public abstract class TimedHostedService : IHostedService
    {
        protected readonly IServiceProvider ServiceProvider;
        protected CronExpression Expression;
        protected Timer Timer;

        protected TimedHostedService(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        protected abstract string ExpressionString { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var overrideExpression = CronExpression.Parse(ExpressionString);
            // If there are no addition settings to override the start cron, it will run at 00:00 for everyday 
            this.Expression = overrideExpression == null ? CronExpression.Parse("@daily") : overrideExpression;

            var now = DateTime.UtcNow;
            var nextRunTime = this.Expression.GetNextOccurrence(now);
            if (nextRunTime == null)
            {
                return Task.CompletedTask;
            }

            var diffMinutes = nextRunTime.Value.Subtract(now).TotalMinutes;
            this.Timer = new Timer(DoWork, null, TimeSpan.FromMinutes(diffMinutes), TimeSpan.Zero);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        protected abstract Task ProcessInScopeAsync();

        private async void DoWork(object state)
        {
            // Process
            await ProcessInScopeAsync();

            // Planning for the next run time
            ScheduleAction();
        }

        private void ScheduleAction()
        {
            var now = DateTime.UtcNow;
            var nextRunTime = this.Expression.GetNextOccurrence(now);
            if (nextRunTime == null)
            {
                this.Timer.Change(Timeout.Infinite, 0);

                return;
            }

            var diffMinutes = nextRunTime.Value.Subtract(now).TotalMinutes;

            this.Timer.Change(TimeSpan.FromMinutes(diffMinutes), TimeSpan.Zero);
        }
    }
}