using NoiseEngine.Jobs;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Physics.Collision;

internal readonly record struct CollisionDetectionThreadStorage(
    List<ConcurrentBag<ColliderData>> ColliderDataBuffer
) : IThreadStorage<CollisionDetectionThreadStorage> {

    public static CollisionDetectionThreadStorage Create(EntitySystem<CollisionDetectionThreadStorage> entitySystem) {
        return new CollisionDetectionThreadStorage(new List<ConcurrentBag<ColliderData>>());
    }

}
