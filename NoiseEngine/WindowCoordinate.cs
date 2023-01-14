using System.Runtime.InteropServices;

namespace NoiseEngine;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct WindowCoordinate {

    public static WindowCoordinate Default => new WindowCoordinate(0, WindowCoordinateMode.Default);
    public static WindowCoordinate Center => new WindowCoordinate(0, WindowCoordinateMode.Center);

    private readonly int value;
    private readonly WindowCoordinateMode mode;

    private WindowCoordinate(int value, WindowCoordinateMode mode) {
        this.value = value;
        this.mode = mode;
    }

    public static implicit operator WindowCoordinate(int value) {
        return new WindowCoordinate(value, WindowCoordinateMode.Value);
    }

}
