using System.ComponentModel.DataAnnotations;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.EnvironmentVariable
{
    public class EnvironmentVariableTaskViewModel
    {
        [Required]
        public string VariableName { get; set; } = "Sync";
    }
}