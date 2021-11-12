using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    public class EntitySystem<T> : EntitySystem where T : struct, IEntityComponent {

        private Dictionary<Entity, T>? components1;

        internal override void Init(EntityWorld world) {
            base.Init(world);

            components1 = world.ComponentsStorage.GetStorage<T>();
        }

        internal void SetComponent(Entity entity, T component) {
            ComponentsStorage.SetComponent(components1!, entity, component);
        }

    }
}
