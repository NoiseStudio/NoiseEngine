using System;

namespace NoiseEngine.Rendering.Exceptions;

public class GraphicsApiNotSupportedException : GraphicsException {

    public GraphicsApi? Api { get; }

    public GraphicsApiNotSupportedException() : base("Given graphics API is not supported.") {
    }

    public GraphicsApiNotSupportedException(GraphicsApi api) : base(GetMessage(api)) {
        Api = api;
    }

    public GraphicsApiNotSupportedException(GraphicsApi api, string? message) : base($"{GetMessage(api)} {message}") {
        Api = api;
    }

    public GraphicsApiNotSupportedException(
        GraphicsApi api, string? message, Exception? innerException
    ) : base($"{GetMessage(api)} {message}", innerException) {
        Api = api;
    }

    private static string GetMessage(GraphicsApi api) {
        return $"{api} graphics API is not supported.";
    }

}
