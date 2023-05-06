using System;

namespace NoiseEngine.Jobs2;

internal readonly record struct ChangedObserverContext(EntityWorld.ChangedObserverInvoker Invoker, Delegate Observer);
