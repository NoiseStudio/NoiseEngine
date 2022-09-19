using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

/// <summary>
/// Unmanaged memory buffer used for passing data to and from the native library.
/// </summary>
/// <typeparam name="T">Type of the element.</typeparam>
public struct InteropArray<T> : IDisposable where T : unmanaged {

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
        pointer = (T*)Marshal.AllocHGlobal(length * Marshal.SizeOf<T>());
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
    public unsafe Span<T> AsSpan() {
        return new Span<T>(pointer, Length);
    }

    /// <summary>
    /// Creates a view into the native memory starting at the index <paramref name="start"/>.
    /// Span becomes invalid when the object is disposed.
    /// </summary>
    /// <param name="start">Start index of the span.</param>
    /// <returns>Span with the view of the memory held by this object.</returns>
    /// <throws cref="IndexOutOfRangeException"><paramref name="start"/> is out of range.</throws>
    public unsafe Span<T> AsSpan(int start) {
        if (start < 0 || start > Length) {
            throw new IndexOutOfRangeException();
        }

        return new Span<T>(pointer + start, Length - start);
    }

    /// <summary>
    /// Creates a view into the native memory starting at the index <paramref name="start"/>.
    /// Span becomes invalid when the object is disposed.
    /// </summary>
    /// <param name="start">Start index of the span.</param>
    /// <param name="length">Lenght of the span.</param>
    /// <returns>Span with the view of the memory held by this object.</returns>
    /// <throws cref="IndexOutOfRangeException">
    /// <paramref name="start"/> is out of range or <paramref name="length"/> is invalid.
    /// </throws>
    public unsafe Span<T> AsSpan(int start, int length) {
        if (start < 0 || start > Length || length < 0 || length > Length - start) {
            throw new IndexOutOfRangeException();
        }

        return new Span<T>(pointer + start, length);
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public unsafe void Dispose() {
        if (pointer == null)
            return;

        Marshal.FreeHGlobal((IntPtr)pointer);
        pointer = null;
        Length = -1;
    }

}
