using NoiseEngine.Serialization;
using System;

namespace NoiseEngine.Tests.Serialization;

public class SerializationReaderTest {

    private readonly SerializationWriter[] writers = new SerializationWriter[] {
        new SerializationWriter(true), new SerializationWriter(false)
    };

    [Theory]
    [InlineData(new byte[] { 1, 2, 3, 4 })]
    public void ReadBytes(byte[] value) {
        TestReader(x => x.WriteBytes(value), x => {
            byte[] readBytes = new byte[x.Count];
            x.ReadBytes(readBytes.AsSpan());
            Assert.Equal(value, x);
        });
    }

    [Theory]
    [InlineData("Hello world!")]
    public void ReadString(string value) {
        TestReader(x => x.WriteString(value), x => Assert.Equal(value, x.ReadString()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadBool(bool value) {
        TestReader(x => x.WriteBool(value), x => Assert.Equal(value, x.ReadBool()));
    }

    [Theory]
    [InlineData(18)]
    public void ReadUInt8(byte value) {
        TestReader(x => x.WriteUInt8(value), x => Assert.Equal(value, x.ReadUInt8()));
    }

    [Theory]
    [InlineData(1864)]
    public void ReadUInt16(ushort value) {
        TestReader(x => x.WriteUInt16(value), x => Assert.Equal(value, x.ReadUInt16()));
    }

    [Theory]
    [InlineData(21072022)]
    public void ReadUInt32(uint value) {
        TestReader(x => x.WriteUInt32(value), x => Assert.Equal(value, x.ReadUInt32()));
    }

    [Theory]
    [InlineData(202220182016)]
    public void ReadUInt64(ulong value) {
        TestReader(x => x.WriteUInt64(value), x => Assert.Equal(value, x.ReadUInt64()));
    }

    [Theory]
    [InlineData(-18)]
    public void ReadInt8(sbyte value) {
        TestReader(x => x.WriteInt8(value), x => Assert.Equal(value, x.ReadInt8()));
    }

    [Theory]
    [InlineData(-1864)]
    public void ReadInt16(short value) {
        TestReader(x => x.WriteInt16(value), x => Assert.Equal(value, x.ReadInt16()));
    }

    [Theory]
    [InlineData(-21072022)]
    public void ReadInt32(int value) {
        TestReader(x => x.WriteInt32(value), x => Assert.Equal(value, x.ReadInt32()));
    }

    [Theory]
    [InlineData(-202220182016)]
    public void ReadInt64(long value) {
        TestReader(x => x.WriteInt64(value), x => Assert.Equal(value, x.ReadInt64()));
    }

    [Theory]
    [InlineData(18.64f)]
    public void ReadFloat16(float value) {
        TestReader(x => x.WriteFloat16((Half)value), x => Assert.Equal((Half)value, x.ReadFloat16()));
    }

    [Theory]
    [InlineData(18.64f)]
    public void ReadFloat32(float value) {
        TestReader(x => x.WriteFloat32(value), x => Assert.Equal(value, x.ReadFloat32()));
    }

    [Theory]
    [InlineData(2022.0910)]
    public void ReadFloat64(double value) {
        TestReader(x => x.WriteFloat64(value), x => Assert.Equal(value, x.ReadFloat64()));
    }

    [Fact]
    public void ToArray() {
        TestReader(x => x.WriteBool(true), x => {
            Assert.Single(x);
            x.ReadBool();
        });
    }

    private void TestReader(Action<SerializationWriter> writeData, Action<SerializationReader> readData) {
        foreach (SerializationWriter writer in writers) {
            writer.Clear();
            writeData(writer);

            SerializationReader reader = new SerializationReader(writer.ToArray(), writer.IsLittleEndian);
            readData(reader);

            Assert.Equal(reader.Count, reader.Position);
        }
    }

}
