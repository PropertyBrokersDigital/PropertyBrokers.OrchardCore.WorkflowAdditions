using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalytics
{   
    public class GoogleAnalyticsTaskViewModel
    {
        public string MeasurementId { get; set; }
        public string ApiSecret { get; set; }
        public string ClientId { get; set; }
        public string EventName { get; set; }
        public string EventParameters { get; set; }
    }
}
