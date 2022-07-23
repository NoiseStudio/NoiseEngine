using System;
using System.Collections;
using System.Collections.Concurrent;

namespace NoiseEngine.Jobs;

internal class ComponentsStorage<TKey> where TKey : notnull {

    private readonly ConcurrentDictionary<Type, IDictionary> storage = new ConcurrentDictionary<Type, IDictionary>();

    internal static void SetComponent<T>(ConcurrentDictionary<TKey, T> storage, TKey key, T component) {
        storage[key] = component;
    }

    internal void Clear() {
        storage.Clear();
    }

    internal ConcurrentDictionary<TKey, T> AddStorage<T>() {
        return (ConcurrentDictionary<TKey, T>)storage.GetOrAdd(typeof(T), _ => new ConcurrentDictionary<TKey, T>());
    }

    internal ConcurrentDictionary<TKey, T> GetStorage<T>() {
        return (ConcurrentDictionary<TKey, T>)GetStorageWithoutCast(typeof(T));
    }

    internal IDictionary GetStorageWithoutCast<T>() {
        return storage[typeof(T)];
    }

    internal IDictionary GetStorageWithoutCast(Type componentType) {
        return storage[componentType];
    }

    internal void AddComponent<T>(TKey key, T component) {
        ConcurrentDictionary<TKey, T> dictionary = AddStorage<T>();
        dictionary.TryAdd(key, component);
    }

    internal void RemoveComponent<T>(TKey key) {
        IDictionary dictionary = GetStorageWithoutCast<T>();
        dictionary.Remove(key);
    }

    internal void RemoveComponent(Type componentType, TKey key) {
        IDictionary dictionary = GetStorageWithoutCast(componentType);
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
