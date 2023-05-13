using System;

namespace NoiseEngine.Jobs2;

internal sealed class JobEmpty : Job {

    private Action? method;

    public override Delegate? Delegate => method;

    public JobEmpty(JobsWorld world, long rawTime, Action method) : base(world, rawTime) {
        this.method = method;
    }

    private protected override void DisposeWorker() {
        method = null;
    }

    private protected override void InvokeWorker() {
        method?.Invoke();
    }

}
