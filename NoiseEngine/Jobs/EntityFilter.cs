using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Jobs;

public class EntityFilter : IEntityFilter {

    private readonly Type[] withComponents;
    private readonly Type[] withoutComponents;

    public IEnumerable<Type> WithComponents => withComponents;
    public IEnumerable<Type> WithoutComponents => withoutComponents;

    public EntityFilter(IEnumerable<Type> withComponents) {
        this.withComponents = withComponents.ToArray();
        withoutComponents = Array.Empty<Type>();
    }

    public EntityFilter(IEnumerable<Type> withComponents, IEnumerable<Type> withoutComponents) {
        this.withComponents = withComponents.ToArray();
        this.withoutComponents = withoutComponents.ToArray();
    }

    /// <summary>
    /// Checks that the <see cref="ComponentType"/>s of <see cref="IComponent"/> meet the requirements of this <see cref="IEntityFilter"/>.
    /// </summary>
    /// <param name="componentTypes"><see cref="ComponentType"/>s of <see cref="IComponent"/>.</param>
    /// <returns>
    /// If <paramref name="componentTypes"/> meet the filter's requirements returns <see langword="true"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool CompareComponents(IEnumerable<ComponentType> componentTypes) {
        foreach (Type componentType in withComponents) {
            if (!componentTypes.Select(x => x.Type).Contains(componentType))
                return false;
        }

        foreach (Type componentType in withoutComponents) {
            if (componentTypes.Select(x => x.Type).Contains(componentType))
                return false;
        }

        return true;
    }

}
