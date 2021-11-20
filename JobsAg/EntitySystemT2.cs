using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    public abstract class EntitySystem<T1, T2> : EntitySystemBase
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {

        private Dictionary<Entity, T1>? components1;
        private Dictionary<Entity, T2>? components2;

        internal override void InternalExecute() {
            base.InternalExecute();

            for (int i = 0; i < groups.Count; i++) {
                EntityGroup group = groups[i];
                for (int j = 0; j < group.entities.Count; j++) {
                    Entity entity = group.entities[j];

                    UpdateEntity(entity, components1![entity], components2![entity]);
                }
            }
        }

        internal override void RegisterGroup(EntityGroup group) {
            if (group.HasComponent(typeof(T1)) && group.HasComponent(typeof(T2)))
                base.RegisterGroup(group);
        }

        internal override void InternalInitialize(EntityWorld world) {
            components1 = world.ComponentsStorage.AddStorage<T1>();
            components2 = world.ComponentsStorage.AddStorage<T2>();

            base.InternalInitialize(world);
        }

        internal void SetComponent(Entity entity, T1 component) {
            ComponentsStorage.SetComponent(components1!, entity, component);
        }

        internal void SetComponent(Entity entity, T2 component) {
            ComponentsStorage.SetComponent(components2!, entity, component);
        }

        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        /// <param name="component1">Component of the operated <see cref="Entity"/></param>
        /// <param name="component2">Component of the operated <see cref="Entity"/></param>
        protected abstract void UpdateEntity(Entity entity, T1 component1, T2 component2);

    }
}
