using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.SaveFileToMedia
{
    public class SaveToMediaTaskViewModel
    {
        [Required]
        public string FileUrl { get; set; }
        [Required]
        public string MediaPath { get; set; }
        public bool SaveWithNoFile { get; set; }
    }

}
