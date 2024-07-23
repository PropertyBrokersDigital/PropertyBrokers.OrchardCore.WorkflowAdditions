using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Workflows.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.SaveFileToMedia
{
    public class SaveFileToMediaTaskViewModel : ActivityViewModel<SaveFileToMediaTask>
    {
        [Required]
        public string FileUrl { get; set; }
        [Required]
        public string MediaPath { get; set; }
        public string FileName { get; set; }
    }

}
