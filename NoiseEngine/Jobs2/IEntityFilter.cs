using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs2;

public interface IEntityFilter {

    /// <summary>
    /// Checks that the types of <see cref="IComponent"/> meet the requirements of this <see cref="IEntityFilter"/>.
    /// </summary>
    /// <param name="componentTypes">Types of <see cref="IComponent"/>.</param>
    /// <returns>
    /// If <paramref name="componentTypes"/> meet the filter's requirements returns <see langword="true"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool CompareComponents(IEnumerable<Type> componentTypes);

}
