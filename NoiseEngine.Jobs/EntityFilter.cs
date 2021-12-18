using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NoiseEngine.Jobs {
    public class EntityFilter : IEntityFilter {

        private readonly Type[] withComponents;
        private readonly HashSet<Type> withoutComponents;

        public EntityFilter(Type[] withComponents) {
            this.withComponents = (Type[])withComponents.Clone();
            withoutComponents = new HashSet<Type>();
        }

        public EntityFilter(Type[] withComponents, Type[] withoutComponents) {
            this.withComponents = (Type[])withComponents.Clone();
            this.withoutComponents = new HashSet<Type>(withoutComponents);
        }

        /// <summary>
        /// Checks that the types of <see cref="IEntityComponent"/> meet the requirements of this filter.
        /// </summary>
        /// <param name="componentTypes">Types of <see cref="IEntityComponent"/>.</param>
        /// <returns>If <see langword="true"/>, then the types of <see cref="IEntityComponent"/> meet the filter's requirements, if <see langword="false"/>, they do not.</returns>
        public bool CompareComponents(ReadOnlyCollection<Type> componentTypes) {
            for (int i = 0; i < withComponents.Length; i++) {
                Type componentType = withComponents[i];
                if (!componentTypes.Contains(componentType))
                    return false;
            }
            for (int i = 0; i < componentTypes.Count; i++) {
                Type componentType = componentTypes[i];
                if (withoutComponents.Contains(componentType))
                    return false;
            }
            return true;
        }

    }
}
