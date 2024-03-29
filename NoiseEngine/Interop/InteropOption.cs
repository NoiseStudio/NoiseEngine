﻿using NoiseEngine.Interop.InteropMarshalling;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct InteropOption<T> where T : unmanaged {

    private readonly InteropBool hasValue;
    private readonly T value;

    public bool HasValue => hasValue;
    public T Value {
        get {
            if (!HasValue)
                throw new InvalidOperationException($"This {nameof(InteropOption<T>)} does not have value.");
            return value;
        }
    }

    public InteropOption(T value) {
        hasValue = true;
        this.value = value;
    }

    public InteropOption(T? value) {
        hasValue = value.HasValue;
        this.value = value.GetValueOrDefault();
    }

    public bool TryGetValue(out T value) {
        value = this.value;
        return hasValue;
    }

    public bool Equals(InteropOption<T> other) {
        return HasValue == other.HasValue && (!HasValue || value.Equals(other.value));
    }

    public override int GetHashCode() {
        if (!HasValue)
            return typeof(T).GetHashCode();
        return value.GetHashCode();
    }

    public static implicit operator InteropOption<T>(T value) {
        return new InteropOption<T>(value);
    }

    public static implicit operator InteropOption<T>(T? value) {
        return new InteropOption<T>(value);
    }

    public static implicit operator T?(InteropOption<T> option) {
        return option.HasValue ? option.value : null;
    }

}
