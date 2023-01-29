using NoiseEngine.Mathematics;

namespace NoiseEngine.Inputs;

public class WindowInput {

    private WindowInputRaw raw;
    private Vector2<double> cursorPosition;
    private Vector2<double> lastCursorPosition;

    public Window Window { get; }
    public Vector2<double> ScrollDelta => raw.ScrollDelta;

    public Vector2<double> CursorPosition {
        get => cursorPosition;
    }

    public Vector2<double> CursorPositionDelta { get; private set; }

    internal WindowInput(Window window) {
        Window = window;
    }

    internal unsafe ref WindowInputRaw ProcessBeforePoolEvents() {
        lastCursorPosition = CursorPosition;
        return ref raw;
    }

    internal void ProcessAfterPoolEvents() {
        cursorPosition = raw.CursorPosition;
        CursorPositionDelta = cursorPosition - lastCursorPosition;
    }

}
