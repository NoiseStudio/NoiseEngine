using NoiseEngine.Jobs;
using System;

namespace NoiseEngine.Components {
    /// <summary>
    /// <see cref="IEntityComponent"/> for using <see cref="EntitySystem{T}"/> without iterating through entities.
    /// </summary>
    /// <remarks>No <see cref="Entity"/> should have this component.</remarks>
    public readonly struct NotUsedComponent : IEntityComponent {

        [Obsolete($"Do not create this component. No {nameof(Entity)} should have this component.", true)]
        public NotUsedComponent() {
            throw new InvalidOperationException();
        }

    }
}
