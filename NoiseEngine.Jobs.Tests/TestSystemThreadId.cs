using System.Threading;

namespace NoiseEngine.Jobs.Tests {
    internal class TestSystemThreadId : EntitySystem<TestComponentA> {

        private int[] adder = null!;
        private int entityCount;

        public int AverageTestComponentAAValue { get; private set; } = 0;

        protected override void OnScheduleChange() {
            adder = new int[ThreadCount];
        }

        protected override void Update() {
            entityCount = 0;
        }

        protected override void UpdateEntity(Entity entity, TestComponentA testComponentA) {
            adder[ThreadId] += testComponentA.A;
            Interlocked.Increment(ref entityCount);
        }

        protected override void LateUpdate() {
            int a = 0;
            for (int i = 0; i < adder.Length; i++)
                a += adder[i];
            AverageTestComponentAAValue = a / entityCount;
        }

    }
}
