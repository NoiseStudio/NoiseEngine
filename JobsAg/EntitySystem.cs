namespace NoiseStudio.JobsAg {
    public abstract class EntitySystem : EntitySystemBase {

        internal override void Execute() {
            base.Execute();

            for (int i = 0; i < groups.Count; i++) {
                EntityGroup group = groups[i];
                for (int j = 0; j < group.entities.Count; j++) {
                    Entity entity = group.entities[j];
                    UpdateEntity(entity);
                }
            }
        }
        
        /// <summary>
        /// This method is executed every cycle of this system on every <see cref="Entity"/> assigned to this system
        /// </summary>
        /// <param name="entity">Operated <see cref="Entity"/></param>
        protected abstract void UpdateEntity(Entity entity);

    }
}
