using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsOutOfMemoryException : Exception {

    public GraphicsOutOfMemoryException() {
    }

    public GraphicsOutOfMemoryException(string? message) : base(message) {
    }

    public GraphicsOutOfMemoryException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
