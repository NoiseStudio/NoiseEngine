namespace NoiseEngine.Jobs.Tests {
    internal class TestSystemA : EntitySystem {

        private int a = -5;
        private int b = -6;

        public int C { get; private set; } = -11;

        protected override void OnInitialize() {
            a = 0;
        }

        protected override void OnStart() {
            b = a;
            a += 4;

            C = -5;
        }

        protected override void OnUpdate() {
            C = b;
        }

        protected override void OnUpdateEntity(Entity entity) {
            C++;
        }

        protected override void OnStop() {
            a += 100;
        }

    }
}
