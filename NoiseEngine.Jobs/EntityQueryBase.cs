using System.Collections.Generic;

namespace NoiseEngine.Jobs {
    public abstract class EntityQueryBase {

        internal readonly ConcurrentList<EntityGroup> groups = new ConcurrentList<EntityGroup>();

        private IEntityFilter? filter;

        public IEntityFilter? Filter {
            get {
                return filter;
            }
            set {
                filter = value;
                World.RegisterGroupsToQuery(this);
            }
        }

        public EntityWorld World { get; private set; }

        public IEnumerable<Entity> Entities => GetEntityEnumerable();

        public EntityQueryBase(EntityWorld world, IEntityFilter? filter) {
            World = world;
            Filter = filter;

            World.AddQuery(this);
        }

        ~EntityQueryBase() {
            World.RemoveQuery(this);
        }

        internal virtual void RegisterGroup(EntityGroup group) {
            if (filter == null || filter.CompareComponents(group))
                groups.Add(group);
        }

        private IEnumerable<Entity> GetEntityEnumerable() {
            foreach (EntityGroup group in groups) {
                group.Wait();

                for (int i = 0; i < group.entities.Count; i++) {
                    yield return group.entities[i];
                }

                group.ReleaseWork();
            }
        }

    }
}
