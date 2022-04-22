using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs {
    public abstract class EntityQueryBase : IDisposable {

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
        public bool IsDisposed { get; private set; }
        public bool IsReadOnly => false;

        public IEnumerable<Entity> Entities => GetEntityEnumerable();

        public EntityQueryBase(EntityWorld world, IEntityFilter? filter = null) {
            World = world;
            Filter = filter;

            World.AddQuery(this);
        }

        ~EntityQueryBase() {
            ReleaseResources();
        }

        /// <summary>
        /// This <see cref="EntityQueryBase"/> will be disposed
        /// </summary>
        public void Dispose() {
            lock (this) {
                if (IsDisposed)
                    return;

                IsDisposed = true;
            }

            ReleaseResources();
            GC.SuppressFinalize(this);
        }

        internal virtual void RegisterGroup(EntityGroup group) {
            if (filter == null || filter.CompareComponents(group))
                groups.Add(group);
        }

        private IEnumerable<Entity> GetEntityEnumerable() {
            foreach (EntityGroup group in groups) {
                group.OrderWorkAndWait();

                if (IsReadOnly) {
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

        private void ReleaseResources() {
            World.RemoveQuery(this);
        }

    }
}
