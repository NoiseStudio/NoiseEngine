using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsOutOfDeviceMemoryException : GraphicsOutOfMemoryException {

    public GraphicsOutOfDeviceMemoryException() {
    }

    public GraphicsOutOfDeviceMemoryException(string? message) : base(message) {
    }

    public GraphicsOutOfDeviceMemoryException(
        string? message, Exception? innerException
    ) : base(message, innerException) {
    }

}
