using System.ComponentModel.DataAnnotations;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleTagManager
{
    public class GoogleTagManagerTaskViewModel
    {
        [Required]
        public string ContainerId { get; set; }

        [Required]
        public string EventExpression { get; set; }

        [Required]
        public string DataLayerExpression { get; set; }
    }
}