namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalyticsEvent
{
    public class GoogleAnalyticsEventTaskViewModel
    {
        public string MeasurementId { get; set; }
        public string ApiSecret { get; set; }
        public string SessionId { get; set; }
        public string ClientId { get; set; }
        public string EventTimeStamp { get; set; }   
        public string RequestTimeStamp { get; set; }  //Questionable if these should be strings or longs... 
        public string EventName { get; set; }
        public string EventParamsExpression { get; set; }
    }
}