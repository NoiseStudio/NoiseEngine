namespace NoiseStudio.JobsAg {
    public class EntitySystem {

        public EntityWorld World { get; private set; } = EntityWorld.Empty;

        internal virtual void Init(EntityWorld world) {
            World = world;
        }

    }
}
