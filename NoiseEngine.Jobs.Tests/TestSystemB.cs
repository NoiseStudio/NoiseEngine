namespace NoiseEngine.Jobs.Tests {
    internal class TestSystemB : EntitySystem<TestComponentA> {

        private int a = -5;
        private int b = -6;
        private int c = -11;

        protected override void Initialize() {
            a = 0;
        }

        protected override void Start() {
            b = a;
            a += 4;
        }

        protected override void Update() {
            c = b;
        }

        protected override void UpdateEntity(Entity entity, TestComponentA component1) {
            component1.A = c++;
            entity.Set(this, component1);
        }

        protected override void Stop() {
            a += 100;
        }

    }
}
