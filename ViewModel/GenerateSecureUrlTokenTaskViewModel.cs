using OrchardCore.Workflows.ViewModels;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken.ViewModels
{
    public class GenerateSecureUrlTokenTaskViewModel : ActivityViewModel<GenerateSecureUrlTokenTask>
    {
        public string Email { get; set; }
        public string ValidityInHours { get; set; }
    }
}