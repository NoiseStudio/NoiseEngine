using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs.Tests {
    internal class TestSystemC : EntitySystem<TestComponentA, TestComponentB> {

        private int a = -5;
        private int b = -6;
        private int c = -11;

        public override IReadOnlyList<Type> WritableComponents { get; } = new Type[] {
            typeof(TestComponentA), typeof(TestComponentB)
        };

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

        protected override void OnUpdateEntity(Entity entity, TestComponentA component1, TestComponentB component2) {
            component1.A = c++;
            component2.A = component1.A + 3;
            entity.Set(this, component1);
            entity.Set(this, component2);
        }

        protected override void OnStop() {
            a += 100;
        }

    }
}
