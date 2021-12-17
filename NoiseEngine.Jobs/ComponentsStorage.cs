using System;
using System.Collections;
using System.Collections.Generic;

namespace NoiseEngine.Jobs {
    internal class ComponentsStorage<TKey> where TKey : notnull {

        private readonly Dictionary<Type, IDictionary> storage = new Dictionary<Type, IDictionary>();

        internal static void SetComponent<T>(Dictionary<TKey, T> storage, TKey key, T component) {
            storage[key] = component;
        }

        internal Dictionary<TKey, T> AddStorage<T>() {
            Type type = typeof(T);
            lock (storage) {
                if (storage.TryGetValue(type, out IDictionary? value))
                    return (Dictionary<TKey, T>)value;

                Dictionary<TKey, T> dictionary = new Dictionary<TKey, T>();
                storage.Add(type, dictionary);

                return dictionary;
            }
        }

        internal Dictionary<TKey, T> GetStorage<T>() {
            return (Dictionary<TKey, T>)GetStorageWithoutCast(typeof(T));
        }

        internal IDictionary GetStorageWithoutCast<T>() {
            return storage[typeof(T)];
        }

        internal IDictionary GetStorageWithoutCast(Type componentType) {
            return storage[componentType];
        }

        internal void AddComponent<T>(TKey key, T component) {
            Dictionary<TKey, T> dictionary = AddStorage<T>();
            lock (dictionary)
                dictionary.Add(key, component);
        }

        internal void RemoveComponent<T>(TKey key) {
            IDictionary dictionary = GetStorageWithoutCast<T>();
            lock (dictionary)
                dictionary.Remove(key);
        }

        internal void RemoveComponent(Type componentType, TKey key) {
            IDictionary dictionary = GetStorageWithoutCast(componentType);
            lock (dictionary)
                dictionary.Remove(key);
        }

        internal void SetComponent<T>(TKey key, T component) {
            GetStorageWithoutCast<T>()[key] = component;
        }

        internal T GetComponent<T>(TKey key) {
            return (T)GetStorageWithoutCast<T>()[key]!;
        }

        internal object GetComponent(TKey key, Type componentType) {
            return GetStorageWithoutCast(componentType)[key]!;
        }

        internal object PopComponent(TKey key, Type componentType) {
            IDictionary dictionary = GetStorageWithoutCast(componentType);
            object obj = dictionary[key]!;
            dictionary.Remove(key);
            return obj;
        }

    }
}
