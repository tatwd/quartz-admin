using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Quartz.Admin.AspNetCoreReactWebHosting
{
    public class HttpSendJob : IJob
    {
        private readonly ILogger<HttpSendJob> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpSendJob(ILogger<HttpSendJob> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogDebug("ok {0}", DateTime.Now);

            // var httpClient = _httpClientFactory.CreateClient();
            // httpClient

            return Task.CompletedTask;
        }
    }
}