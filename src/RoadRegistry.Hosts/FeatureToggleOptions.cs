namespace RoadRegistry.Hosts
{
    using FeatureToggle;

    public class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool UseSnapshotRebuildFeature { get; set; }
    }

    public class UseSnapshotRebuildFeatureToggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseSnapshotRebuildFeatureToggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }
}
