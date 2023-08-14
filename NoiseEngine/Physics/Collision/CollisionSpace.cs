using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal sealed class CollisionSpace {

    private readonly ConcurrentBag<ColliderData> colliders = new ConcurrentBag<ColliderData>();

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void GetNearColliders(List<ConcurrentBag<ColliderData>> colliderDataBuffer) {
        colliderDataBuffer.Add(colliders);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void RegisterCollider(ColliderData collider) {
        colliders.Add(collider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void ClearColliders() {
        colliders.Clear();
    }

}
