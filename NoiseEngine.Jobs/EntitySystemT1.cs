using System.Collections.Generic;

namespace NoiseEngine.Jobs {
    public abstract class EntitySystem<T> : EntitySystemBase where T : struct, IEntityComponent {

        private Dictionary<Entity, T>? components1;

        internal override void InternalExecute() {
            base.InternalExecute();

            foreach (EntityGroup group in groups) {
                for (int j = 0; j < group.entities.Count; j++) {
                    Entity entity = group.entities[j];
                    InternalUpdateEntity(entity);
                }
            }

            ReleaseWork();
        }

        internal override void InternalUpdateEntity(Entity entity) {
            OnUpdateEntity(entity, components1![entity]);
        }

        internal override void RegisterGroup(EntityGroup group) {
            if (group.HasComponent(typeof(T)))
                base.RegisterGroup(group);
        }

        internal override void InternalInitialize(EntityWorld world, EntitySchedule schedule) {
            components1 = world.ComponentsStorage.AddStorage<T>();

            base.InternalInitialize(world, schedule);
        }

        internal void SetComponent(Entity entity, T component) {
            ComponentsStorage<Entity>.SetComponent(components1!, entity, component);
        }

        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        /// <param name="component1">Component of the operated <see cref="Entity"/></param>
        protected abstract void OnUpdateEntity(Entity entity, T component1);

    }
}
