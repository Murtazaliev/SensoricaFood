using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Quartz;
using Quartz.Spi;

namespace AVBDelivery.Jobs
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;
        public JobFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return new JobWrapper(serviceProvider, bundle.JobDetail.JobType);
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
    
    public class JobWrapper : IJob, IDisposable
    {
        private readonly IServiceScope _serviceScope;
        private readonly IJob _job;

        public JobWrapper(IServiceProvider serviceProvider, Type jobType)
        {

            _serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            _job = ActivatorUtilities.CreateInstance(_serviceScope.ServiceProvider, jobType) as IJob;

        }

        public Task Execute(IJobExecutionContext context)
        {
            return _job?.Execute(context);
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}
