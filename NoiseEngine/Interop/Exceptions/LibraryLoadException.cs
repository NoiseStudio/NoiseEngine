using System;

namespace NoiseEngine.Interop.Exceptions;

public class LibraryLoadException : Exception {

    public LibraryLoadException() : base("Unable to load library.") {
    }

    public LibraryLoadException(string? message) : base(message) {
    }

    public LibraryLoadException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
