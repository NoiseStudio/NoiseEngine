using NoiseEngine.Jobs;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class CollisionDetectionSystem : EntitySystem {

    private readonly CollisionSpace space;

    public CollisionDetectionSystem(CollisionSpace space) {
        this.space = space;
    }

    private void OnUpdateEntity(RigidBodyDataComponent data, ColliderComponent collider) {
        switch (collider.Type) {
            case ColliderType.Sphere:
                FromSphere();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FromSphere() {

    }

}
