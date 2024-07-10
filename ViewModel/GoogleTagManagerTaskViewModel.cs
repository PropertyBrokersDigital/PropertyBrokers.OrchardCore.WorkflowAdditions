namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleTagManager
{
    public class GoogleTagManagerTaskViewModel
    {
        public string MeasurementId { get; set; }
        public string ApiSecret { get; set; }
        public string ClientId { get; set; }
        public string EventName { get; set; }
        public string EventParamsExpression { get; set; }
    }
}