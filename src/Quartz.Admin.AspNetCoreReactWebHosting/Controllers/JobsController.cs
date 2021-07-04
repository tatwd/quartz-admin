using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly CoreService _coreService;

        public JobsController(ISchedulerFactory schedulerFactory,
            JobStoreContext jobStoreContext,
            CoreService coreService)
        {
            _schedulerFactory = schedulerFactory;
            _jobStoreContext = jobStoreContext;
            _coreService = coreService;
        }

        // [HttpGet]
        // public async Task<IActionResult> GetAll()
        // {
        //     var scheduler = await _schedulerFactory.GetScheduler();
        //     var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        //     // var triggerKeys =
        //     //     await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(SchedulerConstants.DefaultGroup));
        //
        //     var jobs = new List<object>();
        //     foreach (var key in jobKeys)
        //     {
        //         var jobDetail = await scheduler.GetJobDetail(key);
        //         if (jobDetail == null) continue;
        //         var triggers = (await scheduler.GetTriggersOfJob(key))
        //             .Select(trigger => new
        //             {
        //                 triggerKey = trigger.Key.ToString(),
        //                 startTimeUtc = trigger.StartTimeUtc,
        //                 prevFireTimeUtc = trigger.GetPreviousFireTimeUtc(),
        //                 nextFireTimeUtc = trigger.GetNextFireTimeUtc()
        //             });
        //         jobs.Add(new
        //         {
        //             jobName = key.Name,
        //             jobGroup = key.Group,
        //             jobDesc = jobDetail.Description,
        //             triggers
        //         });
        //     }
        //     return Ok(jobs);
        // }

        [HttpGet("{jobId}/triggers")]
        public async Task<IActionResult> GetTriggers([Required]int jobId, CancellationToken cancellationToken)
        {
            var jobSetting =
                await _jobStoreContext.JobSettings.FirstOrDefaultAsync(i => i.Id == jobId,
                    cancellationToken);
            if (jobSetting == null)
            {
                return Ok(new {code = 1, message = $"Not found job setting by id {jobId.ToString()}"});
            }
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var jobKey = jobSetting.GetQuartzJobKey();
            var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);

            var result = new List<TriggerInfoDto>();
            foreach (var trigger in triggers)
            {
                var state = await scheduler.GetTriggerState(trigger.Key, cancellationToken);
                var item = new TriggerInfoDto(trigger)
                {
                    TriggerState = state.ToString()
                };
                result.Add(item);
            }

            return Ok(result);
        }

        [HttpPost("{jobId}/triggers")]
        public async Task<IActionResult> CreateTrigger([Required] int jobId, CancellationToken cancellationToken)
        {
            var jobSetting =
                await _jobStoreContext.JobSettings.FirstOrDefaultAsync(i => i.Id == jobId,
                    cancellationToken);
            if (jobSetting == null)
            {
                return Ok(new {code = 1, message = $"Not found job setting by id {jobId.ToString()}"});
            }

            await _coreService.CreateJobTrigger(jobSetting, cancellationToken);
            return Ok(new { code = 0, message = "ok" });
        }

        [HttpPost("pause")]
        public async Task<IActionResult> PauseJobs([FromBody] int[] jobSettingIds,
            CancellationToken cancellationToken)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var settings = _jobStoreContext.JobSettings
                .Where(i => jobSettingIds.Contains(i.Id));
            foreach (var setting in settings)
            {
                if (setting.State == JobState.Paused)
                    continue;

                setting.State = JobState.Paused; // make to delete state
                var jobKey = setting.GetQuartzJobKey();

                await scheduler.PauseJob(jobKey, cancellationToken);
                _jobStoreContext.JobSettings.UpdateRange(settings);
            }
            await _jobStoreContext.SaveChangesAsync(cancellationToken);
            return Ok(new { code = 0, message = "ok" });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteJobs([FromBody] int[] jobSettingIds,
            CancellationToken cancellationToken)
        {
            var settings = _jobStoreContext.JobSettings
                .Where(i => jobSettingIds.Contains(i.Id));
            var jobKeys = new List<JobKey>();
            foreach (var setting in settings)
            {
                if (setting.State == JobState.Deleted)
                    continue;

                setting.State = JobState.Deleted; // make to delete state
                jobKeys.Add(setting.GetQuartzJobKey());
            }
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var isOk = await scheduler.DeleteJobs(jobKeys, cancellationToken);
            if (isOk)
            {
                _jobStoreContext.JobSettings.UpdateRange(settings);
                await _jobStoreContext.SaveChangesAsync(cancellationToken);
                return Ok(new { code = 0, message = "ok" });
            }
            return Ok(new { code = 1, message = "Delete Quartz Jobs fail!" });
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetJobSettings(int? id,  int? page, int? limit)
        {
            if (id.HasValue)
            {
                var setting = await _jobStoreContext.JobSettings.FindAsync(id.Value);
                return Ok(new { code = 0, message = "ok", detail = setting });
            }

            var take = limit ?? 10;
            var skip = ((page ?? 1) - 1) * take;

            var settings = _jobStoreContext.JobSettings
                .Where(s => s.State != JobState.Deleted)
                .OrderByDescending(s => s.CreateTime)
                .Skip(skip)
                .Take(take)
                .ToList();
            return Ok(new { code = 0, message = "ok", detail = settings });
        }

        [HttpPost("settings")]
        public async Task<IActionResult> CreateOrUpdateJobSetting([FromBody][Required]JobSettingCreateOrUpdateDto dto,
            CancellationToken cancellationToken)
        {
            JobSetting newJobSetting;
            if (dto.Id.HasValue)
            {
                newJobSetting =
                    await _jobStoreContext.JobSettings.FirstOrDefaultAsync(i => i.Id == dto.Id.Value,
                        cancellationToken);
                if (newJobSetting == null)
                {
                    return BadRequest(new { code = 1404, message = "Cannot update, not found job setting" });
                }
                dto.UpdateJobSetting(newJobSetting);
                _jobStoreContext.Update(newJobSetting);
            }
            else
            {
                newJobSetting = dto.NewJobSetting();
                await _jobStoreContext.JobSettings.AddAsync(newJobSetting, cancellationToken);
            }
            await _jobStoreContext.SaveChangesAsync(cancellationToken);
            return Ok(new { code = 0, message = "ok" });
        }

        [HttpGet("validexpr")]
        public IActionResult ValidExpr(string expr, string type)
        {
            if (string.IsNullOrEmpty(expr))
                return BadRequest(new { code = 1400, message = "不能为空" });

            string msg;
            bool isValid;

            if (type == JobTriggerType.Simple)
            {
                isValid = IsValidSimpleExpr(expr, out msg);
            }
            else
            {
                isValid = CronExpression.IsValidExpression(expr);
                msg = isValid ? "valid" : "Cron expression is invalid";
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

            var values = expr.Split(new [] { '|' }, StringSplitOptions.RemoveEmptyEntries);
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