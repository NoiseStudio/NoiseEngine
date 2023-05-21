using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializationUsed {

    private readonly Dictionary<NeslType, HashSet<NeslType>> inner = new Dictionary<NeslType, HashSet<NeslType>>();

    public IEnumerable<NeslType> OrderedTypes => inner.OrderBy(x => Order(x.Value)).Select(x => x.Key);
    public IEnumerable<NeslType> Types => inner.Keys;

    public void Register(NeslType key) {
        GetContainer(key);
    }

    public void Add(NeslType key, NeslType value) {
        if (key != value && value is not NeslGenericTypeParameter)
            GetContainer(key).Add(value);
    }

    public void Add(NeslType key, IEnumerable<NeslType> values) {
        HashSet<NeslType> set = GetContainer(key);
        foreach (NeslType value in values)
            set.Add(value);
    }

    private HashSet<NeslType> GetContainer(NeslType key) {
        if (!inner.TryGetValue(key, out HashSet<NeslType>? values)) {
            values = new HashSet<NeslType>();
            inner.Add(key, values);
        }
        return values;
    }

    private int Order(HashSet<NeslType> set) {
        return OrderWorker(set, new HashSet<HashSet<NeslType>>());
    }

    private int OrderWorker(HashSet<NeslType> set, HashSet<HashSet<NeslType>> c) {
        int count = set.Count;

        c.Add(set);
        foreach (NeslType type in set) {
            if (inner.TryGetValue(type, out HashSet<NeslType>? next) && c.Add(next))
                count += OrderWorker(next, c);
        }

        return count;
    }

}
