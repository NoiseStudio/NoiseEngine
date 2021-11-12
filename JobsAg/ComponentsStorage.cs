using System;
using System.Collections;
using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    internal class ComponentsStorage {

        private readonly Dictionary<Type, IDictionary> storage = new Dictionary<Type, IDictionary>();

        internal static void SetComponent<T>(Dictionary<Entity, T> storage, Entity entity, T component) where T : struct, IEntityComponent {
            storage[entity] = component;
        }

        internal Dictionary<Entity, T> AddStorage<T>() where T : struct, IEntityComponent {
            Type type = typeof(T);
            lock (storage) {
                if (storage.TryGetValue(type, out IDictionary? value))
                    return (Dictionary<Entity, T>)value;

                Dictionary<Entity, T> dictionary = new Dictionary<Entity, T>();
                storage.Add(type, dictionary);

                return dictionary;
            }
        }

        internal Dictionary<Entity, T> GetStorage<T>() where T : struct, IEntityComponent {
            return (Dictionary<Entity, T>)storage[typeof(T)];
        }

        internal void AddComponent<T>(Entity entity, T component) where T : struct, IEntityComponent {
            Dictionary<Entity, T> dictionary = AddStorage<T>();
            lock (dictionary)
                dictionary.Add(entity, component);
        }

        internal void RemoveComponent<T>(Entity entity) where T : struct, IEntityComponent {
            Dictionary<Entity, T> dictionary = GetStorage<T>();
            lock (dictionary)
                dictionary.Remove(entity);
        }

        internal void SetComponent<T>(Entity entity, T component) where T : struct, IEntityComponent {
            GetStorage<T>()[entity] = component;
        }

        internal T GetComponent<T>(Entity entity) where T : struct, IEntityComponent {
            return GetStorage<T>()[entity];
        }

    }
}
