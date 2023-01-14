namespace NoiseEngine;

public readonly record struct WindowPosition(WindowCoordinate X, WindowCoordinate Y) {

    public static WindowPosition Default => new WindowPosition(WindowCoordinate.Default, WindowCoordinate.Default);
    public static WindowPosition Center => new WindowPosition(WindowCoordinate.Center, WindowCoordinate.Center);

}
