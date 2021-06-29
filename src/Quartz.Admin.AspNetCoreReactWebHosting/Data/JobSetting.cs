using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quartz.Admin.AspNetCoreReactWebHosting.Data
{
    public class JobSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string JobName { get; set; }

        [Required]
        public string JobGroup { get; set; }

        [Required]
        public string JobDesc { get; set; }

        [Required]
        public string TriggerType { get; set; }

        [Required]
        public string StartupType { get; set; }

        [Required]
        public string TriggerExpr { get; set; }

        [Required]
        public string HttpApiUrl { get; set; }

        [Required]
        public string HttpMethod { get; set; }

        public string HttpContentType { get; set; }
        public string HttpBody { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public DateTime CreateTime { get; set; }


        public JobKey GetQuartzJobKey()
        {
            var jobIdStr = Id.ToString();
            var jobKey = new JobKey($"{jobIdStr}", Constants.DefaultGroupName);
            return jobKey;
        }

    }
}