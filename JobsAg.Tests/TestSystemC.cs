namespace NoiseStudio.JobsAg.Tests {
    internal class TestSystemC : EntitySystem<TestComponentA, TestComponentB> {

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

        protected override void UpdateEntity(Entity entity, TestComponentA component1, TestComponentB component2) {
            component1.A = c++;
            component2.A = component1.A + 3;
            entity.Set(this, component1);
            entity.Set(this, component2);
        }

        protected override void Stop() {
            a += 100;
        }

    }
}
