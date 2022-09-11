using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Serialization;

public class SerializationWriterTest {

    private static SerializationWriter littleEndian = new SerializationWriter(true);
    private static SerializationWriter bigEndian = new SerializationWriter(false);

    [Theory]
    [InlineData(new byte[] { 5, 8, 23, 5 }, true)]
    [InlineData(new byte[] { 5, 8, 23, 5 }, false)]
    public void WriteBytesCollection(ICollection<byte> bytes, bool isLittleEndian) {
        AssertBytes(x => x.WriteBytes(bytes), bytes, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 5, 8, 23, 5 }, true)]
    [InlineData(new byte[] { 5, 8, 23, 5 }, false)]
    public void WriteBytesReadOnlySpan(byte[] bytes, bool isLittleEndian) {
        AssertBytes(x => x.WriteBytes(bytes.AsSpan()), bytes, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 5, 8, 23, 5 }, true)]
    [InlineData(new byte[] { 5, 8, 23, 5 }, false)]
    public void WriteBytesArray(byte[] bytes, bool isLittleEndian) {
        AssertBytes(x => x.WriteBytes(bytes), bytes, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0x3, 0x0, 0x0, 0x0, 0x61, 0x62, 0x63 }, "abc", true)]
    [InlineData(new byte[] { 0x0, 0x0, 0x0, 0x3, 0x61, 0x62, 0x63 }, "abc", false)]
    public void WriteString(byte[] expected, string value, bool isLittleEndian) {
        AssertBytes(x => x.WriteString(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0x0 }, false, true)]
    [InlineData(new byte[] { 0x1 }, true, false)]
    public void WriteBool(byte[] expected, bool value, bool isLittleEndian) {
        AssertBytes(x => x.WriteBool(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 53 }, 53, true)]
    [InlineData(new byte[] { 36 }, 36, false)]
    public void WriteUInt8(byte[] expected, byte value, bool isLittleEndian) {
        AssertBytes(x => x.WriteUInt8(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0x49, 0x7d }, 32073, true)]
    [InlineData(new byte[] { 0x7d, 0x49 }, 32073, false)]
    public void WriteUInt16(byte[] expected, ushort value, bool isLittleEndian) {
        AssertBytes(x => x.WriteUInt16(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0xaa, 0x38, 0x2c, 0xbf }, 3207346346, true)]
    [InlineData(new byte[] { 0xbf, 0x2c, 0x38, 0xaa }, 3207346346, false)]
    public void WriteUInt32(byte[] expected, uint value, bool isLittleEndian) {
        AssertBytes(x => x.WriteUInt32(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0x29, 0x42, 0xfb, 0x27, 0xb8, 0x86, 0xea, 0x06 }, 498358834280743465, true)]
    [InlineData(new byte[] { 0x06, 0xea, 0x86, 0xb8, 0x27, 0xfb, 0x42, 0x29 }, 498358834280743465, false)]
    public void WriteUInt64(byte[] expected, ulong value, bool isLittleEndian) {
        AssertBytes(x => x.WriteUInt64(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0xcb }, -53, true)]
    [InlineData(new byte[] { 0xdc }, -36, false)]
    public void WriteInt8(byte[] expected, sbyte value, bool isLittleEndian) {
        AssertBytes(x => x.WriteInt8(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0xb7, 0x82 }, -32073, true)]
    [InlineData(new byte[] { 0x82, 0xb7 }, -32073, false)]
    public void WriteInt16(byte[] expected, short value, bool isLittleEndian) {
        AssertBytes(x => x.WriteInt16(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0x56, 0xfa, 0xe1, 0xec }, -320734634, true)]
    [InlineData(new byte[] { 0xec, 0xe1, 0xfa, 0x56 }, -320734634, false)]
    public void WriteInt32(byte[] expected, int value, bool isLittleEndian) {
        AssertBytes(x => x.WriteInt32(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0xd7, 0xbd, 0x04, 0xd8, 0x47, 0x79, 0x15, 0xf9 }, -498358834280743465, true)]
    [InlineData(new byte[] { 0xf9, 0x15, 0x79, 0x47, 0xd8, 0x04, 0xbd, 0xd7 }, -498358834280743465, false)]
    public void WriteInt64(byte[] expected, long value, bool isLittleEndian) {
        AssertBytes(x => x.WriteInt64(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0xa9, 0x54 }, 74.57f, true)]
    [InlineData(new byte[] { 0x54, 0xa9 }, 74.57f, false)]
    public void WriteFloat16(byte[] expected, float value, bool isLittleEndian) {
        AssertBytes(x => x.WriteFloat16((Half)value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0xb8, 0x06, 0xe9, 0x44 }, 1864.21f, true)]
    [InlineData(new byte[] { 0x44, 0xe9, 0x06, 0xb8 }, 1864.21f, false)]
    public void WriteFloat32(byte[] expected, float value, bool isLittleEndian) {
        AssertBytes(x => x.WriteFloat32(value), expected, isLittleEndian);
    }

    [Theory]
    [InlineData(new byte[] { 0x06, 0x96, 0x0a, 0xc7, 0xd7, 0x20, 0x9d, 0x40 }, 1864.21072022, true)]
    [InlineData(new byte[] { 0x40, 0x9d, 0x20, 0xd7, 0xc7, 0x0a, 0x96, 0x06 }, 1864.21072022, false)]
    public void WriteFloat64(byte[] expected, double value, bool isLittleEndian) {
        AssertBytes(x => x.WriteFloat64(value), expected, isLittleEndian);
    }

    [Fact]
    public void Clear() {
        TestWriter(x => {
            x.WriteBool(true);
            Assert.Single(x);
            x.Clear();
            Assert.Empty(x);
        });
    }

    [Fact]
    public void ToArray() {
        TestWriter(x => {
            x.WriteBool(true);
            Assert.Equal(x, x.ToArray());
        });
    }

    [Fact]
    public void AsSpan() {
        TestWriter(x => {
            x.WriteBool(true);
            Assert.Equal(x, x.AsSpan().ToArray());
        });
    }

    [Fact]
    public void AsSpanStart() {
        TestWriter(x => {
            x.WriteInt32(1864);
            Assert.Equal(2, x.AsSpan(2).Length);
        });
    }

    [Fact]
    public void AsSpanStartLength() {
        TestWriter(x => {
            x.WriteFloat64(1864.21072022);
            Assert.Equal(3, x.AsSpan(2, 3).Length);
        });
    }

    private void AssertBytes(Action<SerializationWriter> factory, IEnumerable<byte> bytes, bool isLittleEndian) {
        SerializationWriter writer = GetWriter(isLittleEndian);
        factory(writer);
        Assert.Equal(bytes, writer);
    }

    private void TestWriter(Action<SerializationWriter> test) {
        littleEndian.Clear();
        test(littleEndian);

        bigEndian.Clear();
        test(bigEndian);
    }

    private SerializationWriter GetWriter(bool isLittleEndian) {
        SerializationWriter writer = isLittleEndian ? littleEndian : bigEndian;
        writer.Clear();
        return writer;
    }

}
