using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quartz.Admin.AspNetCoreReactWebHosting.Data
{
    public class JobLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public DateTime CreateTime { get; set; }

        [StringLength(1024)]
        public string Result { get; set; }
    }
}