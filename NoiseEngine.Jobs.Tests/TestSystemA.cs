namespace NoiseEngine.Jobs.Tests {
    internal class TestSystemA : EntitySystem {

        private int a = -5;
        private int b = -6;

        public int C { get; private set; } = -11;

        protected override void Initialize() {
            a = 0;
        }

        protected override void Start() {
            b = a;
            a += 4;

            C = -5;
        }

        protected override void Update() {
            C = b;
        }

        protected override void UpdateEntity(Entity entity) {
            C++;
        }

        protected override void Stop() {
            a += 100;
        }

    }
}
