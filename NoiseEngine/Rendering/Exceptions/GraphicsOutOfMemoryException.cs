﻿using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsOutOfMemoryException : GraphicsException {

    public GraphicsOutOfMemoryException() : base("Out of graphics memory.") {
    }

    public GraphicsOutOfMemoryException(string? message) : base(message) {
    }

    public GraphicsOutOfMemoryException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
