using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsInstanceCreateException : GraphicsException {

    public GraphicsInstanceCreateException() : base($"Unable to create new {nameof(GraphicsInstance)}.") {
    }

    public GraphicsInstanceCreateException(string? message) : base(message) {
    }

    public GraphicsInstanceCreateException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
