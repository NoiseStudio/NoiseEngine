using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsOutOfDeviceMemoryException : GraphicsOutOfMemoryException {

    public GraphicsOutOfDeviceMemoryException() : base("Out of graphics device memory.") {
    }

    public GraphicsOutOfDeviceMemoryException(string? message) : base(message) {
    }

    public GraphicsOutOfDeviceMemoryException(
        string? message, Exception? innerException
    ) : base(message, innerException) {
    }

}
