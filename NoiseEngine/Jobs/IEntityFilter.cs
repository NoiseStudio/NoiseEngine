using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs;

public interface IEntityFilter {

    /// <summary>
    /// Checks that the types of <see cref="IEntityComponent"/> meet the requirements of this filter.
    /// </summary>
    /// <param name="componentTypes">Types of <see cref="IEntityComponent"/>.</param>
    /// <returns>If <see langword="true"/>, then the types of <see cref="IEntityComponent"/> meet the filter's requirements, if <see langword="false"/>, they do not.</returns>
    public bool CompareComponents(IReadOnlyList<Type> componentTypes);

    internal bool CompareComponents(EntityGroup group) {
        return CompareComponents(group.ComponentTypes);
    }

}
