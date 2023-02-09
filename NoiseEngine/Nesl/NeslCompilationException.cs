using System;

namespace NoiseEngine.Nesl;

public class NeslCompilationException : Exception {

    public NeslCompilationException() : base("Unknown NESL compilation exception.") {
    }

    public NeslCompilationException(string? message) : base(message) {
    }

    public NeslCompilationException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
