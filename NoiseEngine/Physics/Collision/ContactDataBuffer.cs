using NoiseEngine.Jobs;
using System.Collections.Concurrent;

namespace NoiseEngine.Physics.Collision;

internal sealed class ContactDataBuffer {

    public ConcurrentDictionary<(Entity, Entity, int), ContactManifold> Data { get; } =
        new ConcurrentDictionary<(Entity, Entity, int), ContactManifold>();

    public void AddContactPoint(Entity current, Entity other, int convexHullId, ContactPoint point) {
        ContactManifold manifold = Data.GetOrAdd((current, other, convexHullId), default(ContactManifold));
        manifold.AddContactPoint(point);
        Data[(current, other, convexHullId)] = manifold;
    }

}
