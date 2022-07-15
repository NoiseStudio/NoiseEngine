namespace NoiseEngine.Jobs;

internal readonly struct SchedulePackage {

    public EntitySystemBase EntitySystem { get; }
    public EntityGroup? EntityGroup { get; }

    public int PackageStartIndex { get; }
    public int PackageEndIndex { get; }
    public bool IsCycleBegin { get; }

    public SchedulePackage(
        EntitySystemBase entitySystem, EntityGroup entityGroup, int packageStartIndex, int packageEndIndex
    ) {
        EntitySystem = entitySystem;
        EntityGroup = entityGroup;
        PackageStartIndex = packageStartIndex;
        PackageEndIndex = packageEndIndex;
        IsCycleBegin = false;
    }

    public SchedulePackage(EntitySystemBase entitySystem) {
        EntitySystem = entitySystem;
        EntityGroup = null;
        PackageStartIndex = 0;
        PackageEndIndex = 0;
        IsCycleBegin = true;
    }

}
