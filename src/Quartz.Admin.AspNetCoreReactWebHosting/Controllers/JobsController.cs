using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.Matchers;

namespace Quartz.Admin.AspNetCoreReactWebHosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public JobsController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobDetail = JobBuilder.Create<HttpSendJob>()
                .WithIdentity(id, SchedulerConstants.DefaultGroup)
                .WithDescription("testing")
                .StoreDurably()
                .Build();
            await scheduler.AddJob(jobDetail, true, CancellationToken.None);
            return Ok(new {code = 0, message = "ok"});
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>
                .GroupEquals(SchedulerConstants.DefaultGroup));
            // var triggerKeys =
            //     await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(SchedulerConstants.DefaultGroup));

            var jobs = new List<object>();
            foreach (var key in jobKeys)
            {
                var jobDetail = await scheduler.GetJobDetail(key);
                if (jobDetail == null) continue;
                var triggers = (await scheduler.GetTriggersOfJob(key))
                    .Select(trigger => trigger.StartTimeUtc);
                jobs.Add(new
                {
                    jobKey = key.Name,
                    jobGroup = key.Group,
                    jobDesc = jobDetail.Description,
                    triggers = triggers
                });
            }
            return Ok(jobs);
        }

        [HttpGet("{id}/triggers")]
        public async Task<IActionResult> GetTriggers(string id)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey(id, SchedulerConstants.DefaultGroup);
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            return Ok(triggers.Select(i => i.StartTimeUtc));
        }

        [HttpGet("{id}/triggers/{triggerId}")]
        public async Task<IActionResult> CreateTrigger(string id, string triggerId,
            DateTime? startAt, int interval = 5, int repeatCount = 0)
        {
           var jobKey = new JobKey(id, SchedulerConstants.DefaultGroup);
           var scheduler = await _schedulerFactory.GetScheduler();
           if (startAt.HasValue)
           {
               var trigger = TriggerBuilder.Create()
                   .ForJob(jobKey)
                   .WithIdentity(triggerId, SchedulerConstants.DefaultGroup)
                   .WithDescription("testing trigger")
                   .StartAt(startAt.Value)
                   .WithSimpleSchedule(x => x
                       .WithInterval(TimeSpan.FromSeconds(interval))
                       .WithRepeatCount(repeatCount))
                   .Build();
               await scheduler.ScheduleJob(trigger, CancellationToken.None);
               return Ok(new {code = 0, message = "ok"});
           }
           return Ok(new {code=1,message="no trigger create"});
        }
    }
}