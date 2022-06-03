namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RequestRoadNetworkSnapshotRefresh")]
    [EventDescription("Indicates that a refresh of the road network snapshot got requested")]
    public class RequestRoadNetworkSnapshotRefresh
    {
        public string When { get; set; }
    }
}
