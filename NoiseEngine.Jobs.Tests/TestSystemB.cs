namespace NoiseEngine.Jobs.Tests {
    internal class TestSystemB : EntitySystem<TestComponentA> {

        private int a = -5;
        private int b = -6;
        private int c = -11;

        protected override void OnInitialize() {
            a = 0;
        }

        protected override void OnStart() {
            b = a;
            a += 4;
        }

        protected override void OnUpdate() {
            c = b;
        }

        protected override void OnUpdateEntity(Entity entity, TestComponentA component1) {
            component1.A = c++;
            entity.Set(this, component1);
        }

        protected override void OnStop() {
            a += 100;
        }

    }
}
