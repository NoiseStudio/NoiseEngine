using NoiseEngine.Jobs;
using System;

namespace NoiseEngine.Physics.Collision;

internal sealed partial class SolverSystem : EntitySystem {

    private float invertedCycleTime;

    protected override void OnStart() {
        invertedCycleTime = (float)(1000 / (CycleTime ?? throw new InvalidOperationException(
            $"{nameof(CycleTime)} is null."
        )));
    }

    protected override void OnUpdate() {
        PreStep();
        for (int i = 0; i < 16; i++)
            Step();
    }

    private void PreStep() {

    }

    private void Step() {

    }

}
