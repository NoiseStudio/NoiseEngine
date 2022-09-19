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
        pointer = (T*) Marshal.AllocHGlobal(length * Marshal.SizeOf<T>());
        Length = length;
    }

    /// <summary>
    /// Copies the contents of the array to the new <see cref="InteropArray{T}"/>.
    /// </summary>
    /// <param name="array">Array to use.</param>
    public unsafe InteropArray(T[] array) : this(array.Length) {
        array.CopyTo(new Span<T>(pointer, Length));
    }

    /// <summary>
    /// Creates a view into the native memory. Span becomes invalid when the object is disposed.
    /// </summary>
    /// <returns>Span with the view of the memory held by this object.</returns>
    public unsafe Span<T> AsSpan() {
        return new Span<T>(pointer, Length);
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public unsafe void Dispose() {
        if (pointer == null)
            return;

        Marshal.FreeHGlobal((IntPtr) pointer);
        pointer = null;
        Length = -1;
    }

}
