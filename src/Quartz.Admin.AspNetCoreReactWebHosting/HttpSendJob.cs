using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz.Admin.AspNetCoreReactWebHosting.Data;

namespace Quartz.Admin.AspNetCoreReactWebHosting
{
    [DisallowConcurrentExecution]
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

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var req = new HttpRequestMessage(jobSetting.HttpMethod == "POST" ? HttpMethod.Post : HttpMethod.Get,
                    jobSetting.HttpApiUrl);
                // if (!string.IsNullOrEmpty(jobSetting.HttpContentType))
                // {
                //     req.Headers.Add("Content-Type", jobSetting.HttpContentType);
                // };
                if (!string.IsNullOrEmpty(jobSetting.HttpBody))
                {
                    req.Content = new StringContent(jobSetting.HttpBody);
                }
                var res = await httpClient.SendAsync(req, context.CancellationToken);
                res.EnsureSuccessStatusCode();
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    var resJson = await res.Content.ReadAsStringAsync();
                    _logger.LogDebug("response:{resJson}", resJson);
                }
                _logger.LogDebug("(#{0}) completed ok {1}", jobSettingId, DateTime.Now);

                jobSetting.State = JobState.Completed;
                await _jobStoreContext.SaveChangesAsync(context.CancellationToken);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "jobSetting:{@jobSetting}", jobSetting);
                jobSetting.State = JobState.Exception;
                await _jobStoreContext.SaveChangesAsync(context.CancellationToken);
            }
        }

        public void Dispose()
        {
            _logger.LogDebug("Disposed HttpSendJob!");
            _jobStoreContext?.Dispose();
        }
    }
}