namespace NoiseEngine.Jobs {
    public abstract class EntitySystem : EntitySystemBase {

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
            OnUpdateEntity(entity);
        }
        
        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        protected abstract void OnUpdateEntity(Entity entity);

    }
}
