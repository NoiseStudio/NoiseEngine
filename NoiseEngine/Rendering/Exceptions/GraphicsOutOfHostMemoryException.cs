using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsOutOfHostMemoryException : GraphicsOutOfMemoryException {

    public GraphicsOutOfHostMemoryException() : base("Out of graphics host memory.") {
    }

    public GraphicsOutOfHostMemoryException(string? message) : base(message) {
    }

    public GraphicsOutOfHostMemoryException(
        string? message, Exception? innerException
    ) : base(message, innerException) {
    }

}
