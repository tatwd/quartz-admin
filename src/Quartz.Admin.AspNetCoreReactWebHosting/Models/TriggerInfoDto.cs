using System;

namespace Quartz.Admin.AspNetCoreReactWebHosting.Models
{
    public class TriggerInfoDto
    {
        public string TriggerKey { get; set; }
        public DateTimeOffset StartTimeUtc { get; set; }
        public DateTimeOffset? PrevFireTimeUtc { get; set; }
        public DateTimeOffset? NextFireTimeUtc { get; set; }
        public bool MayFireAgain { get; set; }
        public string TriggerState { get; set; }

        public TriggerInfoDto()
        {
        }

        public TriggerInfoDto(ITrigger trigger)
        {
            TriggerKey = trigger.Key.ToString();
            StartTimeUtc = trigger.StartTimeUtc;
            PrevFireTimeUtc = trigger.GetPreviousFireTimeUtc();
            NextFireTimeUtc = trigger.GetNextFireTimeUtc();
            MayFireAgain = trigger.GetMayFireAgain();
        }
    }
}