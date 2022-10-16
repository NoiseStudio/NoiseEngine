using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsException : Exception {

    public GraphicsException() : base("Unknown graphics exception.") {
    }

    public GraphicsException(string? message) : base(message) {
    }

    public GraphicsException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
