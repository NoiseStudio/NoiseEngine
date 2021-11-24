using System.Threading;

namespace NoiseStudio.JobsAg.Tests {
    internal class TestSystemScheduleA : EntitySystem<TestComponentA, TestComponentB> {

        private int updateEntityCount = 0;

        public int UpdateEntityCount => updateEntityCount;

        public bool UsedUpdate { get; private set; }
        public int UpdateCount { get; private set; } = 0;

        protected override void Update() {
            UsedUpdate = true;
            UpdateCount++;
        }

        protected override void UpdateEntity(Entity entity, TestComponentA component1, TestComponentB component2) {
            Interlocked.Increment(ref updateEntityCount);
            entity.Destroy(World);
        }

    }
}
