using NoiseEngine.Jobs;
using System;

namespace NoiseEngine.Components;

/// <summary>
/// <see cref="IComponent"/> for using <see cref="EntitySystem"/> without iterating through entities.
/// </summary>
/// <remarks>No <see cref="Entity"/> should have this component.</remarks>
public readonly struct NotUsedComponent : IComponent {

    [Obsolete($"Do not create this component. No {nameof(Entity)} should have this component.", true)]
    public NotUsedComponent() {
        throw new InvalidOperationException();
    }

}
