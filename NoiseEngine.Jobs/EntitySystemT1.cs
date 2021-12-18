using System.Collections.Generic;

namespace NoiseEngine.Jobs {
    public abstract class EntitySystem<T> : EntitySystemBase where T : struct, IEntityComponent {

        internal EntityQuery<T>? queryGeneric;

        internal override void InternalExecute() {
            base.InternalExecute();

            foreach ((Entity entity, T component) element in queryGeneric!) {
                OnUpdateEntity(element.entity, element.component);
            }

            ReleaseWork();
        }

        internal override void InternalUpdateEntity(Entity entity) {
            OnUpdateEntity(entity, queryGeneric!.components1[entity]);
        }

        internal override void InternalInitialize(EntityWorld world, EntitySchedule schedule) {
            queryGeneric = new EntityQuery<T>(world, Filter);
            query = queryGeneric;

            base.InternalInitialize(world, schedule);
        }

        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        /// <param name="component1">Component of the operated <see cref="Entity"/></param>
        protected abstract void OnUpdateEntity(Entity entity, T component1);

    }
}
