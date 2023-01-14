using System;

namespace NoiseEngine.Rendering.Exceptions;

public class PresentationNotSupportedException : GraphicsException {

    public PresentationNotSupportedException() : base("Presentation is not supported.") {
    }

    public PresentationNotSupportedException(string? message) : base(message) {
    }

    public PresentationNotSupportedException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
