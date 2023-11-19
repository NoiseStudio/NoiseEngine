using NoiseEngine.Jobs;
using System.Collections.Concurrent;

namespace NoiseEngine.Physics.Collision;

internal sealed class ContactDataBuffer {

    internal ushort frameId;

    public ConcurrentDictionary<(Entity, Entity, int), ContactManifold> Data { get; } =
        new ConcurrentDictionary<(Entity, Entity, int), ContactManifold>();

    public void NextFrame() {
        frameId++;
    }

    public void AddContactPoint(Entity bodyA, Entity bodyB, int convexHullId, ContactPoint point) {
        // NOTE: Optimize this CPU cache.
        /*if (bodyA.GetHashCode() > bodyB.GetHashCode()) {
            (bodyA, bodyB) = (bodyB, bodyA);
            (point.Position, point.PositionB) = (point.PositionB.ToPos(), point.Position.ToFloat());
        }*/

        ContactManifold manifold = Data.GetOrAdd((bodyA, bodyB, convexHullId), default(ContactManifold));
        if (manifold.FrameId + 1 != frameId)
            manifold = default;
        manifold.FrameId = frameId;

        manifold.AddContactPoint(point);
        Data[(bodyA, bodyB, convexHullId)] = manifold;
    }

    public void UpdateContactPoint(Entity bodyA, Entity bodyB, int convexHullId, ContactManifold manifold) {
        Data.AddOrUpdate((bodyA, bodyB, convexHullId), manifold, (_, _) => manifold);
    }

}
