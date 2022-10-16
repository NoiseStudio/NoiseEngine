using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsDeviceLostException : GraphicsException {

    public GraphicsDeviceLostException() : base("Failed to connect to the graphics device.") {
    }

    public GraphicsDeviceLostException(string? message) : base(message) {
    }

    public GraphicsDeviceLostException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
