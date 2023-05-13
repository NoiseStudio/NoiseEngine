namespace NoiseEngine.Jobs2;

internal readonly record struct SchedulePackage(
    bool IsCycleBegin, EntitySystem System, ArchetypeChunk? Chunk, nint StartIndex, nint EndIndex
);
