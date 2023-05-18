using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoiseEngine.Serialization;

public class SerializationWriter : IReadOnlyList<byte> {

    private readonly SerializationWriterDelegation delegation;

    public bool IsLittleEndian { get; }
    public bool IsBigEndian => !IsLittleEndian;

    public int Count => delegation.Data.Count;

    public byte this[int index] {
        get => delegation.Data[index];
        set => delegation.Data[index] = value;
    }

    public SerializationWriter(bool isLittleEndian = true) {
        IsLittleEndian = isLittleEndian;

        if (isLittleEndian)
            delegation = new SerializationWriterDelegationLittleEndian();
        else
            delegation = new SerializationWriterDelegationBigEndian();
    }

    /// <summary>
    /// Writes <see cref="ICollection{T}"/> of <see cref="byte"/>s to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="bytes"><see cref="ICollection{T}"/> of <see cref="byte"/>s.</param>
    public void WriteBytes(ICollection<byte> bytes) {
        delegation.Data.AddRange(bytes);
    }

    /// <summary>
    /// Writes <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/>s to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="bytes"><see cref="ReadOnlySpan{T}"/> of <see cref="byte"/>s.</param>
    public void WriteBytes(ReadOnlySpan<byte> bytes) {
        delegation.Data.AddRange(bytes);
    }

    /// <summary>
    /// Writes array of <see cref="byte"/>s to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="bytes">Array of <see cref="byte"/>s.</param>
    public void WriteBytes(byte[] bytes) {
        delegation.Data.AddRange(bytes);
    }

    /// <summary>
    /// Converts <see cref="string"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="string"/> value.</param>
    public void WriteString(string obj) {
        byte[] bytes = Encoding.UTF8.GetBytes(obj);
        WriteInt32(bytes.Length);
        delegation.Data.AddRange(bytes);
    }

    /// <summary>
    /// Writes <paramref name="enumerable"/> to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="enumerable"><see cref="IEnumerable{T}"/> of <see cref="byte"/>s.</param>
    public void WriteEnumerable(IEnumerable<byte> enumerable) {
        byte[] array = enumerable.ToArray();
        WriteInt32(array.Length);
        delegation.Data.AddRange(array);
    }

    /// <summary>
    /// Writes <paramref name="enumerable"/> to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="enumerable"><see cref="IEnumerable{T}"/> of <see cref="string"/>s.</param>
    public void WriteEnumerable(IEnumerable<string> enumerable) {
        int start = delegation.Data.Count;
        WriteInt32(0);

        int i = 0;
        foreach (string obj in enumerable) {
            WriteString(obj);
            i++;
        }

        Span<byte> span = AsSpan(start);
        if (IsLittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(span, i);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, i);
    }

    /// <summary>
    /// Writes <paramref name="enumerable"/> to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="enumerable"><see cref="IEnumerable{T}"/> of <see cref="ulong"/>s.</param>
    public void WriteEnumerable(IEnumerable<ulong> enumerable) {
        int start = delegation.Data.Count;
        WriteInt32(0);

        int i = 0;
        foreach (ulong obj in enumerable) {
            WriteUInt64(obj);
            i++;
        }

        Span<byte> span = AsSpan(start);
        if (IsLittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(span, i);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, i);
    }

    /// <summary>
    /// Converts <paramref name="enumerable"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <typeparam name="T">Type of entered data.</typeparam>
    /// <param name="enumerable">T values.</param>
    public void WriteEnumerable<T>(IEnumerable<T> enumerable) where T : ISerializable {
        int start = delegation.Data.Count;
        WriteInt32(0);

        int i = 0;
        foreach (T obj in enumerable) {
            WriteObject(obj);
            i++;
        }

        Span<byte> span = AsSpan(start);
        if (IsLittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(span, i);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, i);
    }

    /// <summary>
    /// Converts <see cref="bool"/> to byte and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="bool"/> value.</param>
    public void WriteBool(bool obj) {
        delegation.Data.Add((byte)(obj ? 1 : 0));
    }

    /// <summary>
    /// Writes <see cref="byte"/> it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="byte"/> value.</param>
    public void WriteUInt8(byte obj) {
        delegation.Data.Add(obj);
    }

    /// <summary>
    /// Converts <see cref="ushort"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="ushort"/> value.</param>
    public void WriteUInt16(ushort obj) {
        delegation.WriteUInt16(obj);
    }

    /// <summary>
    /// Converts <see cref="uint"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="uint"/> value.</param>
    public void WriteUInt32(uint obj) {
        delegation.WriteUInt32(obj);
    }

    /// <summary>
    /// Converts <see cref="ulong"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="ulong"/> value.</param>
    public void WriteUInt64(ulong obj) {
        delegation.WriteUInt64(obj);
    }

    /// <summary>
    /// Converts <see cref="nuint"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <remarks>32-bit architectures are not supported.</remarks>
    /// <param name="obj"><see cref="nuint"/> value.</param>
    public void WriteUIntN(nuint obj) {
        WriteUInt64(obj);
    }

    /// <summary>
    /// Converts <see cref="sbyte"/> to byte and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="sbyte"/> value.</param>
    public void WriteInt8(sbyte obj) {
        delegation.Data.Add((byte)obj);
    }

    /// <summary>
    /// Converts <see cref="short"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="short"/> value.</param>
    public void WriteInt16(short obj) {
        delegation.WriteInt16(obj);
    }

    /// <summary>
    /// Converts <see cref="int"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="int"/> value.</param>
    public void WriteInt32(int obj) {
        delegation.WriteInt32(obj);
    }

    /// <summary>
    /// Converts <see cref="long"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="long"/> value.</param>
    public void WriteInt64(long obj) {
        delegation.WriteInt64(obj);
    }

    /// <summary>
    /// Converts <see cref="nint"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <remarks>32-bit architectures are not supported.</remarks>
    /// <param name="obj"><see cref="nint"/> value.</param>
    public void WriteIntN(nint obj) {
        WriteInt64(obj);
    }

    /// <summary>
    /// Converts <see cref="Half"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="Half"/> value.</param>
    public void WriteFloat16(Half obj) {
        delegation.WriteFloat16(obj);
    }

    /// <summary>
    /// Converts <see cref="float"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="float"/> value.</param>
    public void WriteFloat32(float obj) {
        delegation.WriteFloat32(obj);
    }

    /// <summary>
    /// Converts <see cref="double"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="double"/> value.</param>
    public void WriteFloat64(double obj) {
        delegation.WriteFloat64(obj);
    }

    /// <summary>
    /// Converts T to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <typeparam name="T">Type of entered data.</typeparam>
    /// <param name="obj">T value.</param>
    public void WriteObject<T>(T obj) where T : ISerializable {
        obj.Serialize(this);
    }

    /// <summary>
    /// Converts <see cref="ISerializable"/> to bytes and writes it to this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="obj"><see cref="ISerializable"/> value.</param>
    public void WriteObject(ISerializable obj) {
        obj.Serialize(this);
    }

    /// <summary>
    /// Clears this <see cref="SerializationWriter"/>.
    /// </summary>
    public void Clear() {
        delegation.Data.Clear();
    }

    /// <summary>
    /// Copies the bytes of the <see cref="SerializationWriter"/> to a new array.
    /// </summary>
    /// <returns>An array containing copies of the bytes of the <see cref="SerializationWriter"/>.</returns>
    public byte[] ToArray() {
        return delegation.Data.ToArray();
    }

    /// <summary>
    /// Creates a new span over this <see cref="SerializationWriter"/>.
    /// </summary>
    /// <returns>The span representation of the <see cref="SerializationWriter"/>.</returns>
    public Span<byte> AsSpan() {
        return delegation.Data.AsSpan();
    }

    /// <summary>
    /// Creates a new span over a portion of this <see cref="SerializationWriter"/> starting at a specified
    /// position to the end of the <see cref="SerializationWriter"/>.
    /// </summary>
    /// <param name="start">
    /// The initial index from which the <see cref="SerializationWriter"/> will be converted.
    /// </param>
    /// <returns>The span representation of the <see cref="SerializationWriter"/>.</returns>
    public Span<byte> AsSpan(int start) {
        return delegation.Data.AsSpan(start);
    }

    /// <summary>
    /// Creates a new span over a portion of this <see cref="SerializationWriter"/> starting at a specified
    /// position for a specified length.
    /// </summary>
    /// <param name="start">
    /// The initial index from which the <see cref="SerializationWriter"/> will be converted.
    /// </param>
    /// <param name="length">The number of items in the span.</param>
    /// <returns>The span representation of the <see cref="SerializationWriter"/>.</returns>
    public Span<byte> AsSpan(int start, int length) {
        return delegation.Data.AsSpan(start, length);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="SerializationWriter"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the <see cref="SerializationWriter"/>.</returns>
    public IEnumerator<byte> GetEnumerator() {
        return delegation.Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
