using NoiseEngine.Collections;
using NoiseEngine.Jobs;
using NoiseEngine.Physics.Collision.Mesh;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Physics.Collision;

internal readonly record struct CollisionDetectionThreadStorage(
    List<ConcurrentBag<ColliderData>> ColliderDataBuffer,
    Polytope3DBuffer PolytopeBuffer
) : IThreadStorage<CollisionDetectionThreadStorage> {

    public static CollisionDetectionThreadStorage Create(EntitySystem<CollisionDetectionThreadStorage> entitySystem) {
        return new CollisionDetectionThreadStorage(
            new List<ConcurrentBag<ColliderData>>(),
            new Polytope3DBuffer(new FastList<PolytopeFace>(), new FastList<(int, int)>())
        );
    }

}
