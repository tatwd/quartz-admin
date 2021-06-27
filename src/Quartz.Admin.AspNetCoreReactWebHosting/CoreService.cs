using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz.Admin.AspNetCoreReactWebHosting.Data;

namespace Quartz.Admin.AspNetCoreReactWebHosting
{
    public class CoreService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<CoreService> _logger;

        public CoreService(ISchedulerFactory schedulerFactory,
            ILogger<CoreService> logger)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
        }


        public async Task<IJobDetail> GetOrAddHttpSendJobAsync(JobSetting jobSetting, CancellationToken cancellationToken)
        {
            var jobIdStr = jobSetting.Id.ToString();
            var jobKey = new JobKey($"{jobIdStr}_{jobSetting.JobName}", jobSetting.JobGroup);

            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var jobDetail = await scheduler.GetJobDetail(jobKey, cancellationToken);
            if (jobDetail != null)
            {
                return jobDetail;
            }

            jobDetail = JobBuilder.Create<HttpSendJob>()
                .WithIdentity(jobKey)
                .WithDescription(jobSetting.JobDesc)
                .StoreDurably()
                .UsingJobData("jobId", jobIdStr)
                .Build();

            await scheduler.AddJob(jobDetail, true, cancellationToken);
            return jobDetail;
        }

        public async Task CreateJobTrigger(JobSetting jobSetting, CancellationToken cancellationToken)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var jobDetail = await GetOrAddHttpSendJobAsync(jobSetting, cancellationToken);
            ITrigger trigger;

            if (jobSetting.TriggerType == JobTriggerType.Simple)
            {
                trigger = CreateSimpleTrigger(jobDetail, jobSetting.TriggerExpr);
            }
            else if (jobSetting.TriggerType == JobTriggerType.Cron)
            {
                trigger = CreateCornTrigger(jobDetail, jobSetting.TriggerExpr);
            }
            else
            {
                throw new ArgumentException("Unknown value of `TriggerType`", nameof(jobSetting.TriggerType));
            }

            if (await scheduler.CheckExists(trigger.Key, cancellationToken))
            {
                _logger.LogInformation("Already exists trigger `{0}`", trigger.Key);
                // await scheduler.ResumeTrigger(trigger.Key, cancellationToken);
                // await scheduler.TriggerJob(jobDetail.Key, cancellationToken);
                await scheduler.RescheduleJob(trigger.Key, trigger, cancellationToken);
                return;
            }

            // await scheduler.TriggerJob(jobDetail.Key, cancellationToken);
            await scheduler.ScheduleJob(trigger, cancellationToken);
        }

        private static ITrigger CreateSimpleTrigger(IJobDetail jobDetail, string jobTriggerExpr)
        {
            var triggerExpr = jobTriggerExpr.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            var startAt = DateTime.Parse(triggerExpr[0]);
            var interval = int.Parse(triggerExpr[1]);
            var repeatCount = int.Parse(triggerExpr[2]);

            var triggerId = $"{jobDetail.Key.Name}_trigger";
            var trigger = TriggerBuilder.Create()
                .ForJob(jobDetail)
                .WithIdentity(triggerId, jobDetail.Key.Group)
                .WithDescription(jobDetail.Description)
                .StartAt(startAt)
                .WithSimpleSchedule(x => x
                    .WithInterval(TimeSpan.FromSeconds(interval))
                    .WithRepeatCount(repeatCount))
                .Build();

            return trigger;
        }

        private static ITrigger CreateCornTrigger(IJobDetail jobDetail, string jobTriggerExpr)
        {
            var triggerId = $"{jobDetail.Key.Name}_trigger";
            var trigger = TriggerBuilder.Create()
                .ForJob(jobDetail)
                .WithIdentity(triggerId, jobDetail.Key.Group)
                .WithDescription(jobDetail.Description)
                .WithCronSchedule(jobTriggerExpr)
                .Build();

            return trigger;
        }


    }
}