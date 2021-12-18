using System;
using System.Collections.ObjectModel;

namespace NoiseEngine.Jobs {
    public interface IEntityFilter {

        /// <summary>
        /// Checks that the types of <see cref="IEntityComponent"/> meet the requirements of this filter.
        /// </summary>
        /// <param name="componentTypes">Types of <see cref="IEntityComponent"/>.</param>
        /// <returns>If <see langword="true"/>, then the types of <see cref="IEntityComponent"/> meet the filter's requirements, if <see langword="false"/>, they do not.</returns>
        public bool CompareComponents(ReadOnlyCollection<Type> componentTypes);

        internal bool CompareComponents(EntityGroup group) {
            return CompareComponents(group.ComponentTypes);
        }

    }
}
