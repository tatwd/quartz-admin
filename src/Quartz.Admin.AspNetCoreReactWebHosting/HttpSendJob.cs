using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz.Admin.AspNetCoreReactWebHosting.Data;

namespace Quartz.Admin.AspNetCoreReactWebHosting
{
    public class HttpSendJob : IJob, IDisposable
    {
        private readonly ILogger<HttpSendJob> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JobStoreContext _jobStoreContext;

        public HttpSendJob(ILogger<HttpSendJob> logger,
            IHttpClientFactory httpClientFactory,
            JobStoreContext jobStoreContext)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jobStoreContext = jobStoreContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (!context.JobDetail.JobDataMap.TryGetValue(Constants.JobSettingIdName,
                out var jobSettingIdStr) ||
                !int.TryParse(jobSettingIdStr.ToString(), out var jobSettingId))
            {
                _logger.LogWarning("Missing jobSettingId from JobDetail's JobDataMap!");
                return;
            }

            var jobSetting = await _jobStoreContext.JobSettings.FirstOrDefaultAsync(i =>
                i.Id == jobSettingId && i.State != JobState.Deleted, context.CancellationToken);
            if (jobSetting == null)
            {
                _logger.LogWarning("Not found job setting by id equals {0} where state is not deleted!", jobSettingId);
                return;
            }

            jobSetting.State = JobState.Fired;
            await _jobStoreContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogDebug("(#{0}) fired ok {1}", jobSettingIdStr, DateTime.Now);

            // var httpClient = _httpClientFactory.CreateClient();
            // httpClient
            await Task.Delay(TimeSpan.FromSeconds(5));

            _logger.LogDebug("(#{0}) completed ok {1}", jobSettingId, DateTime.Now);

            jobSetting.State = JobState.Completed;
            await _jobStoreContext.SaveChangesAsync(context.CancellationToken);
        }

        public void Dispose()
        {
            _logger.LogDebug("Disposed HttpSendJob!");
            _jobStoreContext?.Dispose();
        }
    }
}