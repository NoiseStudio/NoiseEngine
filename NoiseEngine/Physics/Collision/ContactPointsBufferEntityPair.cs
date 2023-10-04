using NoiseEngine.Jobs;
using System;

namespace NoiseEngine.Physics.Collision;

internal readonly record struct ContactPointsBufferEntityPair(
    Entity EntityA, Entity EntityB
) : IEquatable<ContactPointsBufferEntityPair> {

    public bool Equals(ContactPointsBufferEntityPair other) {
        return (ReferenceEquals(EntityA, other.EntityA) && ReferenceEquals(EntityB, other.EntityB)) ||
            (ReferenceEquals(EntityA, other.EntityB) && ReferenceEquals(EntityB, other.EntityA));
    }

    public override int GetHashCode() {
        return EntityA.GetHashCode() ^ EntityB.GetHashCode();
    }

}
