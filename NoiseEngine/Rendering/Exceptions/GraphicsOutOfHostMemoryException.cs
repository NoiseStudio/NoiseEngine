using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsOutOfHostMemoryException : GraphicsOutOfMemoryException {

    public GraphicsOutOfHostMemoryException() {
    }

    public GraphicsOutOfHostMemoryException(string? message) : base(message) {
    }

    public GraphicsOutOfHostMemoryException(
        string? message, Exception? innerException
    ) : base(message, innerException) {
    }

}
