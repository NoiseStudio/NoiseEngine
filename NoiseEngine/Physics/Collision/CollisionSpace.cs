using NoiseEngine.Jobs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal sealed class CollisionSpace {

    private readonly ConcurrentBag<ColliderData> colliders = new ConcurrentBag<ColliderData>();
    public readonly ConcurrentDictionary<Entity, ColliderTransform> Transforms =
        new ConcurrentDictionary<Entity, ColliderTransform>();

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void GetNearColliders(List<ConcurrentBag<ColliderData>> colliderDataBuffer) {
        colliderDataBuffer.Add(colliders);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ColliderTransform GetTransform(Entity entity) {
        return Transforms[entity];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ColliderTransform UpdateTransform(Entity entity, ColliderTransform transform) {
        return Transforms.AddOrUpdate(entity, transform, (_, _) => transform);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void RegisterCollider(ColliderData collider) {
        colliders.Add(collider);
        UpdateTransform(collider.Entity, collider.Transform);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void ClearColliders() {
        colliders.Clear();
        //Transforms.Clear();
    }

}
