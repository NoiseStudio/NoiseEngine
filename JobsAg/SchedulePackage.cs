namespace NoiseStudio.JobsAg {
    internal struct SchedulePackage {

        public EntitySystemBase EntitySystem { get; }
        public EntityGroup EntityGroup { get; }

        public int PackageStartIndex { get; }
        public int PackageEndIndex { get; }

        public SchedulePackage(EntitySystemBase entitySystem, EntityGroup entityGroup, ScheduleRawPackage rawPackage) {
            EntityGroup = entityGroup;
            EntitySystem = entitySystem;
            PackageStartIndex = rawPackage.PackageStartIndex;
            PackageEndIndex = rawPackage.PackageEndIndex;
        }

    }
}
