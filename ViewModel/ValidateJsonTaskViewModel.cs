using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.ValidateJson
{
    public class ValidateJsonTaskViewModel
    {
        [Required]
        public string SchemaContent { get; set; }
        public string JsonContent { get; set; }
        public string SchemaValidationState { get; set; }
    }
}
