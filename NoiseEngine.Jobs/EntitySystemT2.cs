namespace NoiseEngine.Jobs {
    public abstract class EntitySystem<T1, T2> : EntitySystemBase
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {

        internal EntityQuery<T1, T2>? queryGeneric;

        internal override void InternalExecute() {
            base.InternalExecute();

            foreach ((Entity entity, T1 component1, T2 component2) element in queryGeneric!) {
                OnUpdateEntity(element.entity, element.component1, element.component2);
            }

            ReleaseWork();
        }

        internal override void InternalUpdateEntity(Entity entity) {
            OnUpdateEntity(entity, queryGeneric!.components1![entity], queryGeneric!.components2![entity]);
        }

        internal override void InternalInitialize(EntityWorld world, EntitySchedule? schedule) {
            base.InternalInitialize(world, schedule);

            queryGeneric = new EntityQuery<T1, T2>(world, WritableComponents, Filter);
            query = queryGeneric;
        }

        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        /// <param name="component1">Component of the operated <see cref="Entity"/></param>
        /// <param name="component2">Component of the operated <see cref="Entity"/></param>
        protected abstract void OnUpdateEntity(Entity entity, T1 component1, T2 component2);

    }
}
