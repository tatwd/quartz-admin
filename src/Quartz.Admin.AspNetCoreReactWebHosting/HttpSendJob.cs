using System;
using System.Threading.Tasks;

namespace Quartz.Admin.AspNetCoreReactWebHosting
{
    public class HttpSendJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("ok {0}", DateTime.Now);
            return Task.CompletedTask;
        }
    }
}