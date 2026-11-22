using System.ComponentModel.DataAnnotations;
using OrchardCore.Workflows.ViewModels;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.RunReportToFile
{
    public class RunReportToFileTaskViewModel : ActivityViewModel<RunReportToFileTask>
    {
        [Required]
        public string ReportContentItemId { get; set; }

        [Required]
        public string JsonData { get; set; }

        [Required]
        public string MediaPath { get; set; }

        public string FileName { get; set; }

        public bool Overwrite { get; set; }
    }
}
