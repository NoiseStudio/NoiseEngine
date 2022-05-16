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

        internal override bool InternalInitialize(EntityWorld world, EntitySchedule? schedule) {
            if (!base.InternalInitialize(world, schedule))
                return false;

            queryGeneric = new EntityQuery<T>(world, true, Filter); // TODO: add real isReadOnly param
            query = queryGeneric;

            return true;
        }

        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        /// <param name="component1">Component of the operated <see cref="Entity"/></param>
        protected abstract void OnUpdateEntity(Entity entity, T component1);

    }
}
