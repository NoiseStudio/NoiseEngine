namespace NoiseEngine.Jobs;

internal readonly record struct SchedulePackage(
    bool IsCycleBegin, EntitySystem System, ArchetypeChunk? Chunk, nint StartIndex, nint EndIndex
);
