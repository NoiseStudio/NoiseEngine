using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

/// <summary>
/// Unmanaged memory buffer used for passing data to and from the native library.
/// </summary>
/// <typeparam name="T">Type of the element.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public struct InteropArray<T> : IDisposable, IReadOnlyList<T> where T : unmanaged {

    private unsafe T* pointer;

    /// <summary>
    /// Length of the array.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Gets or sets the element at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Index of the element.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// Index is out of range or object is disposed.
    /// </exception>
    public readonly unsafe T this[int index] {
        get {
            if (index < 0 || index >= Length) {
                throw new IndexOutOfRangeException();
            }

            return pointer[index];
        }
        set {
            if (index < 0 || index >= Length) {
                throw new IndexOutOfRangeException();
            }

            pointer[index] = value;
        }
    }

    int IReadOnlyCollection<T>.Count => Length;

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [Obsolete("Use other constructors instead.", true)]
    public InteropArray() {
        throw new InvalidOperationException();
    }

    /// <summary>
    /// Creates a new <see cref="InteropArray{T}"/> with the specified <paramref name="length"/>.
    /// </summary>
    /// <param name="length">Length of the array.</param>
    public unsafe InteropArray(int length) {
        nuint size = (nuint)Marshal.SizeOf<T>();
        pointer = (T*)NativeMemory.AlignedAlloc(size * (nuint)length, size);
        Length = length;
    }

    /// <summary>
    /// Copies the contents of the <paramref name="array"/> to the new <see cref="InteropArray{T}"/>.
    /// </summary>
    /// <param name="array">Array to use.</param>
    public unsafe InteropArray(T[] array) : this(array.Length) {
        array.CopyTo(new Span<T>(pointer, Length));
    }

    /// <summary>
    /// Copies the contents of the <paramref name="span"/> to the new <see cref="InteropArray{T}"/>.
    /// </summary>
    /// <param name="span">Span to use.</param>
    public unsafe InteropArray(ReadOnlySpan<T> span) : this(span.Length) {
        span.CopyTo(new Span<T>(pointer, Length));
    }

    /// <summary>
    /// Creates a view into the native memory. Span becomes invalid when the object is disposed.
    /// </summary>
    /// <returns>Span with the view of the memory held by this object.</returns>
    public readonly unsafe Span<T> AsSpan() {
        return new Span<T>(pointer, Length);
    }

    /// <summary>
    /// Creates a view into the native memory starting at the index <paramref name="start"/>.
    /// Span becomes invalid when the object is disposed.
    /// </summary>
    /// <param name="start">Start index of the span.</param>
    /// <returns>Span with the view of the memory held by this object.</returns>
    /// <throws cref="ArgumentOutOfRangeException"><paramref name="start"/> is out of range.</throws>
    public readonly unsafe Span<T> AsSpan(int start) {
        if (start < 0 || start > Length) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        return new Span<T>(pointer + start, Length - start);
    }

    /// <summary>
    /// Creates a view into the native memory starting at the index <paramref name="start"/>.
    /// Span becomes invalid when the object is disposed.
    /// </summary>
    /// <param name="start">Start index of the span.</param>
    /// <param name="length">Length of the span.</param>
    /// <returns>Span with the view of the memory held by this object.</returns>
    /// <throws cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is out of range or <paramref name="length"/> is invalid.
    /// </throws>
    public readonly unsafe Span<T> AsSpan(int start, int length) {
        if (start < 0 || start > Length) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (length < 0 || length > Length - start) {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        return new Span<T>(pointer + start, length);
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public unsafe void Dispose() {
        if (pointer == null)
            return;

        NativeMemory.AlignedFree(pointer);
        pointer = null;
        Length = -1;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator() {
        for (int i = 0; i < Length; i++) {
            yield return GetWithoutChecks(i);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe T GetWithoutChecks(int index) {
        return pointer[index];
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
