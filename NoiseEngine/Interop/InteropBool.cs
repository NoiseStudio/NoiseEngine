using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct InteropBool {

    private readonly byte value;

    public bool Value => value != 0;

    public InteropBool(bool value) {
        this.value = value ? (byte)1 : (byte)0;
    }

    public bool Equals(InteropBool other) {
        return Value.Equals(other.Value);
    }

    public override int GetHashCode() {
        return Value.GetHashCode();
    }

    public override string ToString() {
        return Value.ToString();
    }

    public static implicit operator InteropBool(bool value) {
        return new InteropBool(value);
    }

    public static implicit operator bool(InteropBool value) {
        return value.Value;
    }

}
