using System;

namespace NoiseEngine.Rendering.Exceptions;

internal class GraphicsInstanceCreateException : Exception {

    public GraphicsInstanceCreateException() {
    }

    public GraphicsInstanceCreateException(string? message) : base(message) {
    }

    public GraphicsInstanceCreateException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
