namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalyticsManager
{
    public class GoogleAnalyticsTaskViewModel
    {
        public string MeasurementId { get; set; }
        public string ApiSecret { get; set; }
        public string ClientId { get; set; }
        public string EventName { get; set; }
        public string EventParamsExpression { get; set; }
    }
}