using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.ProcessMjmlTemplate
{
    public class ProcessMjmlTemplateTaskViewModel
    {
        [Required]
        public string EmailTemplateContent { get; set; }
        public string MergeTagsContent { get; set; }
    }
}
