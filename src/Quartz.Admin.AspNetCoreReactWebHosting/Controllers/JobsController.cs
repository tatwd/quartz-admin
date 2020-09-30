using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz.Admin.AspNetCoreReactWebHosting.Data;
using Quartz.Admin.AspNetCoreReactWebHosting.Models;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.Matchers;

namespace Quartz.Admin.AspNetCoreReactWebHosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly JobStoreContext _jobStoreContext;

        public JobsController(ISchedulerFactory schedulerFactory,
            JobStoreContext jobStoreContext)
        {
            _schedulerFactory = schedulerFactory;
            _jobStoreContext = jobStoreContext;
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
            return Ok(new { code = 0, message = "ok" });
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
                return Ok(new { code = 0, message = "ok" });
            }
            return Ok(new { code = 1, message = "no trigger create" });
        }

        [HttpPost]
        public IActionResult CreateJob(JobSettingCreateOrUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var newJobSetting = dto.NewJobSetting();
            _jobStoreContext.JobSettings.AddAsync(newJobSetting, cancellationToken);
            _jobStoreContext.SaveChangesAsync(cancellationToken);
            return Ok(new { code = 0, message = "created" });
        }

        [HttpGet("validexpr")]
        public IActionResult ValidExpr(string expr, int type)
        {
            if (string.IsNullOrEmpty(expr))
                return BadRequest(new { code = 1400, message = "不能为空" });

            string msg;
            bool isValid;

            if (type == 0)
            {
                isValid = IsValidSimpleExpr(expr, out msg);
            }
            else
            {
                isValid = CronExpression.IsValidExpression(expr);
                msg = isValid ? "valid" : "Cron 表达无效";
            }
            if (!isValid)
                return BadRequest(new { code = 1400, message = msg });
            return Ok(new { code = 0, message = "valid" });
        }

        /// <summary>
        /// Valid simple trigger value. i.e.
        ///     2020-09-30 03:00|5|2
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool IsValidSimpleExpr(string expr, out string message)
        {
            if (string.IsNullOrEmpty(expr))
            {
                message = "Required";
                return false;
            }

            var values = expr.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 3)
            {
                message = "Must contain 3 parts, split by '|'";
                return false;
            }

            var startAt = values[0];
            var interval = values[1];
            var repeatCount = values[2];

            if (!DateTime.TryParse(startAt, out _))
            {
                message = "First part must format by DateTime type";
                return false;
            }

            if (!int.TryParse(interval, out _))
            {
                message = "Second part must be a integer";
                return false;
            }

            if (!int.TryParse(repeatCount, out _))
            {
                message = "Last part must be a integer";
                return false;
            };
            message = "valid";
            return true;
        }

    }
}