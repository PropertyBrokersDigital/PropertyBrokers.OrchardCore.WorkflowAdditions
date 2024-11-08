namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalyticsEvent
{
    public class GoogleAnalyticsEventTaskViewModel
    {
        public string MeasurementId { get; set; }
        public string ApiSecret { get; set; }
        public string SessionId { get; set; }
        public string ClientId { get; set; }
        public long EventTimeStamp { get; set; }
        public long RequestTimeStamp { get; set; }
        public string EventName { get; set; }
        public string EventParamsExpression { get; set; }
    }
}