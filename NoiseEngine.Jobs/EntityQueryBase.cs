using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs {
    public abstract class EntityQueryBase {

        internal readonly ConcurrentList<EntityGroup> groups = new ConcurrentList<EntityGroup>();

        private readonly WeakReference<EntityQueryBase> queryWeakReference;

        private IEntityFilter? filter;

        public virtual IReadOnlyList<Type> WritableComponents { get; }

        public IEntityFilter? Filter {
            get {
                return filter;
            }
            set {
                filter = value;
                World.RegisterGroupsToQuery(this);
            }
        }

        public EntityWorld World { get; }

        public IEnumerable<Entity> Entities => GetEntityEnumerable();

        public EntityQueryBase(
            EntityWorld world, IReadOnlyList<Type>? writableComponents = null, IEntityFilter? filter = null
        ) {
            World = world;
            WritableComponents = writableComponents ?? Array.Empty<Type>();
            Filter = filter;

            queryWeakReference = new WeakReference<EntityQueryBase>(this);
            World.AddQuery(queryWeakReference);
        }

        ~EntityQueryBase() {
            World.RemoveQuery(queryWeakReference);
        }

        internal virtual void RegisterGroup(EntityGroup group) {
            if (filter == null || filter.CompareComponents(group))
                groups.Add(group);
        }

        private IEnumerable<Entity> GetEntityEnumerable() {
            foreach (EntityGroup group in groups) {
                group.OrderWorkAndWait();

                if (WritableComponents.Count == 0) {
                    for (int i = 0; i < group.Entities.Count; i++) {
                        Entity entity = group.Entities[i];
                        if (entity != Entity.Empty)
                            yield return entity;
                    }
                } else {
                    for (int i = 0; i < group.Entities.Count; i += EntityGroup.PackageSize) {
                        group.EnterWriteLock(i);

                        int maxIndex = Math.Min(i + EntityGroup.PackageSize, group.Entities.Count);
                        for (int j = i; j < maxIndex; j++) {
                            Entity entity = group.Entities[j];
                            if (entity != Entity.Empty)
                                yield return entity;
                        }

                        group.ExitWriteLock(i);
                    }
                }

                group.ReleaseWork();
            }
        }

    }
}
