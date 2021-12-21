using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NoiseEngine.Jobs {
    public class EntityQuery<T> : EntityQueryBase, IEnumerable<(Entity entity, T component)>
        where T : struct, IEntityComponent
    {

        internal readonly ConcurrentDictionary<Entity, T> components1;

        public EntityQuery(EntityWorld world, IEntityFilter? filter = null) : base(world, filter) {
            components1 = world.ComponentsStorage.AddStorage<T>();
        }

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="EntityQuery"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through this <see cref="EntityQuery"/>.</returns>
        public IEnumerator<(Entity entity, T component)> GetEnumerator() {
            foreach (Entity entity in Entities) {
                yield return (entity, components1[entity]);
            }
        }

        internal void SetComponent(Entity entity, T component) {
            ComponentsStorage<Entity>.SetComponent(components1, entity, component);
        }

        internal override void RegisterGroup(EntityGroup group) {
            if (group.HasComponent(typeof(T)))
                base.RegisterGroup(group);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }
}
