namespace NoiseEngine.Jobs.Benchmarks {
    internal class TestSystemA : EntitySystem {

        private int count;

        protected override void OnUpdateEntity(Entity entity) {
            count++;
        }

    }
}
