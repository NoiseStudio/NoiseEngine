using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    public abstract class EntitySystem<T> : EntitySystemBase where T : struct, IEntityComponent {

        private Dictionary<Entity, T>? components1;

        internal override void Execute() {
            base.Execute();

            for (int i = 0; i < groups.Count; i++) {
                EntityGroup group = groups[i];
                for (int j = 0; j < group.entities.Count; j++) {
                    Entity entity = group.entities[j];

                    UpdateEntity(entity, components1![entity]);
                }
            }
        }

        internal override void RegisterGroup(EntityGroup group) {
            if (group.HasComponent(typeof(T)))
                base.RegisterGroup(group);
        }

        internal override void InternalInitialize(EntityWorld world) {
            components1 = world.ComponentsStorage.GetStorage<T>();

            base.InternalInitialize(world);
        }

        internal void SetComponent(Entity entity, T component) {
            ComponentsStorage.SetComponent(components1!, entity, component);
        }

        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        /// <param name="component1">Component of the operated <see cref="Entity"/></param>
        protected abstract void UpdateEntity(Entity entity, T component1);

    }
}
