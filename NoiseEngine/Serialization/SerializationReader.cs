using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoiseEngine.Serialization;

public class SerializationReader : IReadOnlyList<byte> {

    private readonly SerializationReaderDelegation delegation;

    public int Position { get; set; }

    public bool IsLittleEndian { get; }
    public bool IsBigEndian => !IsLittleEndian;

    public int Count => delegation.Count;
    public byte this[int index] => delegation[index];

    public SerializationReader(byte[] data, bool isLittleEndian = true) {
        IsLittleEndian = isLittleEndian;

        if (isLittleEndian)
            delegation = new SerializationReaderDelegationArrayLittleEndian(this, data);
        else
            delegation = new SerializationReaderDelegationArrayBigEndian(this, data);
    }

    public SerializationReader(IEnumerable<byte> data, bool isLittleEndian = true)
        : this(data.ToArray(), isLittleEndian) {
    }

    /// <summary>
    /// Reads <see cref="byte"/>s from this <see cref="SerializationReader"/> and
    /// writes it to given <see cref="Span{T}"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    public void ReadBytes(Span<byte> bytes) {
        delegation.ReadBytes(bytes);
        Position += bytes.Length;
    }

    /// <summary>
    /// Reads <see cref="string"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="string"/>.</returns>
    public string ReadString() {
        int length = ReadInt32();
        Span<byte> span = length <= 1024 ? stackalloc byte[length] : new byte[length];
        ReadBytes(span);
        return Encoding.UTF8.GetString(span);
    }

    /// <summary>
    /// Reads <see cref="bool"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="bool"/>.</returns>
    public bool ReadBool() {
        return ReadUInt8() == 1;
    }

    /// <summary>
    /// Reads <see cref="byte"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="byte"/>.</returns>
    public byte ReadUInt8() {
        return this[Position++];
    }

    /// <summary>
    /// Reads <see cref="ushort"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="ushort"/>.</returns>
    public ushort ReadUInt16() {
        return delegation.ReadUInt16();
    }

    /// <summary>
    /// Reads <see cref="uint"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="uint"/>.</returns>
    public uint ReadUInt32() {
        return delegation.ReadUInt32();
    }

    /// <summary>
    /// Reads <see cref="ulong"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="ulong"/>.</returns>
    public ulong ReadUInt64() {
        return delegation.ReadUInt64();
    }

    /// <summary>
    /// Reads <see cref="sbyte"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="sbyte"/>.</returns>
    public sbyte ReadInt8() {
        return (sbyte)ReadUInt8();
    }

    /// <summary>
    /// Reads <see cref="short"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="short"/>.</returns>
    public short ReadInt16() {
        return delegation.ReadInt16();
    }

    /// <summary>
    /// Reads <see cref="int"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="int"/>.</returns>
    public int ReadInt32() {
        return delegation.ReadInt32();
    }

    /// <summary>
    /// Reads <see cref="long"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="long"/>.</returns>
    public long ReadInt64() {
        return delegation.ReadInt64();
    }

    /// <summary>
    /// Reads <see cref="Half"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="Half"/>.</returns>
    public Half ReadFloat16() {
        return delegation.ReadFloat16();
    }

    /// <summary>
    /// Reads <see cref="float"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="float"/>.</returns>
    public float ReadFloat32() {
        return delegation.ReadFloat32();
    }

    /// <summary>
    /// Reads <see cref="double"/> from this <see cref="SerializationReader"/>.
    /// </summary>
    /// <remarks>
    /// Adds the number of bytes read to the <see cref="Position"/>.
    /// </remarks>
    /// <returns>Read <see cref="double"/>.</returns>
    public double ReadFloat64() {
        return delegation.ReadFloat64();
    }

    /// <summary>
    /// Copies the bytes of this <see cref="SerializationReader"/> to a new array.
    /// </summary>
    /// <returns>An array containing copies of the bytes of this <see cref="SerializationReader"/>.</returns>
    public byte[] ToArray() {
        return delegation.ToArray();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="SerializationReader"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the <see cref="SerializationReader"/>.</returns>
    public IEnumerator<byte> GetEnumerator() {
        return delegation.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
