namespace NoiseEngine.Jobs {
    public abstract class EntitySystem : EntitySystemBase {

        internal EntityQuery? queryGeneric;

        internal override void InternalExecute() {
            base.InternalExecute();

            foreach (Entity entity in queryGeneric!) {
                InternalUpdateEntity(entity);
            }

            ReleaseWork();
        }

        internal override void InternalUpdateEntity(Entity entity) {
            OnUpdateEntity(entity);
        }

        internal override void InternalInitialize(EntityWorld world, EntitySchedule? schedule) {
            queryGeneric = new EntityQuery(world, Filter);
            query = queryGeneric;

            base.InternalInitialize(world, schedule);
        }

        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        protected abstract void OnUpdateEntity(Entity entity);

    }
}
