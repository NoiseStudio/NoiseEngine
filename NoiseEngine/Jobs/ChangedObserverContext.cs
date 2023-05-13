using System;

namespace NoiseEngine.Jobs;

internal readonly record struct ChangedObserverContext(EntityWorld.ChangedObserverInvoker Invoker, Delegate Observer);
