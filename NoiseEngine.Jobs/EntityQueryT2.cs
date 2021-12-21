using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Jobs {
    public class EntityQuery<T1, T2> : EntityQueryBase, IEnumerable<(Entity entity, T1 component1, T2 component2)>
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {

        internal readonly ConcurrentDictionary<Entity, T1> components1;
        internal readonly ConcurrentDictionary<Entity, T2> components2;

        public EntityQuery(EntityWorld world, IEntityFilter? filter = null) : base(world, filter) {
            components1 = world.ComponentsStorage.AddStorage<T1>();
            components2 = world.ComponentsStorage.AddStorage<T2>();
        }

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="EntityQuery"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through this <see cref="EntityQuery"/>.</returns>
        public IEnumerator<(Entity entity, T1 component1, T2 component2)> GetEnumerator() {
            foreach (Entity entity in Entities) {
                yield return (entity, components1[entity], components2[entity]);
            }
        }

        internal void SetComponent(Entity entity, T1 component) {
            ComponentsStorage<Entity>.SetComponent(components1, entity, component);
        }

        internal void SetComponent(Entity entity, T2 component) {
            ComponentsStorage<Entity>.SetComponent(components2, entity, component);
        }

        internal override void RegisterGroup(EntityGroup group) {
            if (group.HasComponent(typeof(T1)) && group.HasComponent(typeof(T2)))
                base.RegisterGroup(group);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }
}
