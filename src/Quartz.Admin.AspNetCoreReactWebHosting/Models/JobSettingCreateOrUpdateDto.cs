using System;
using System.ComponentModel.DataAnnotations;
using Quartz.Admin.AspNetCoreReactWebHosting.Data;

namespace Quartz.Admin.AspNetCoreReactWebHosting.Models
{
    public class JobSettingCreateOrUpdateDto
    {
        /// <summary>
        /// HasValue is true => update else is create
        /// </summary>
        public int? Id { get; set; }

        [Required]
        public string JobName { get; set; }

        [Required]
        public string JobGroup { get; set; }

        [Required]
        public string JobDesc { get; set; }

        [Required]
        public int TriggerType { get; set; }

        [Required]
        public string TriggerExpr { get; set; }

        [Required]
        public string HttpApiUrl { get; set; }

        [Required]
        public string HttpMethod { get; set; }

        public string HttpContentType { get; set; }
        public string HttpBody { get; set; }

        public JobSetting NewJobSetting()
        {
            return new JobSetting
            {
                JobName = JobName,
                JobGroup = JobGroup,
                JobDesc = JobDesc,
                TriggerType = TriggerType,
                TriggerExpr = TriggerExpr,
                HttpApiUrl = HttpApiUrl,
                HttpMethod = HttpMethod,
                HttpContentType = HttpContentType,
                HttpBody = HttpBody,
                State = 0, // init state
                CreateTime = DateTime.Now
            };
        }

        public void UpdateJobSetting(JobSetting jobSetting)
        {
            jobSetting.JobName = JobName;
            jobSetting.JobGroup = JobGroup;
            jobSetting.JobDesc = JobDesc;
            jobSetting.TriggerType = TriggerType;
            jobSetting.TriggerExpr = TriggerExpr;
            jobSetting.HttpApiUrl = HttpApiUrl;
            jobSetting.HttpMethod = HttpMethod;
            jobSetting.HttpContentType = HttpContentType;
            jobSetting.HttpBody = HttpBody;
        }
    }
}