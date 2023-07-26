using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System.Diagnostics;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class CollisionResolveSystem : EntitySystem {

    private readonly ContactPointsBuffer contactPoints;

    public CollisionResolveSystem(ContactPointsBuffer contactPoints) {
        this.contactPoints = contactPoints;
    }

    private void OnUpdateEntity(
        Entity entity, ref RigidBodyComponent rigidBody, ref RigidBodyFinalDataComponent data,
        RigidBodyMiddleDataComponent middle
    ) {
        ContactPointsBufferIterator iterator = contactPoints.IterateThrough(entity);
        while (iterator.MoveNext()) {
            rigidBody.Velocity = rigidBody.Velocity.Scale(iterator.Current.Normal);
            return;
        }

        data.TargetPosition = middle.Position;
    }

}
