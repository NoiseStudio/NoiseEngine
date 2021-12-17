using System.Threading;
using Xunit;

namespace NoiseEngine.Jobs.Tests {
    internal class TestSystemScheduleA : EntitySystem<TestComponentA, TestComponentB> {

        private int updateEntityCount = 0;

        public int UpdateEntityCount => updateEntityCount;

        public bool UsedUpdate { get; private set; }
        public bool UsedLateUpdate { get; private set; }
        public int UpdateCount { get; private set; } = 0;
        public int LateUpdateCount { get; private set; } = 0;

        protected override void Update() {
            UsedUpdate = true;
            UpdateCount++;

            Assert.True(IsWorking);
        }

        protected override void UpdateEntity(Entity entity, TestComponentA component1, TestComponentB component2) {
            Interlocked.Increment(ref updateEntityCount);
            entity.Destroy(World);
        }

        protected override void LateUpdate() {
            UsedLateUpdate = true;
            LateUpdateCount++;

            Assert.True(IsWorking);
        }

    }
}
