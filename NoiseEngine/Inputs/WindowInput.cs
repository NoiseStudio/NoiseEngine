using NoiseEngine.Common;
using NoiseEngine.Interop.Rendering.Presentation;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Inputs;

public class WindowInput {

    private WindowInputRaw raw;
    private Vector2<double> cursorPosition;
    private Vector2<double> lastCursorPosition;
    private bool resetCursorPositionDelta = true;

    public Window Window { get; }
    public CursorLockMode CursorLockMode { get; set; }

    public Vector2<double> CursorPosition {
        get => cursorPosition;
        set {
            cursorPosition = value;
            if (WindowRc.TryRcRetain()) {
                WindowInterop.SetCursorPosition(Window.Handle, value);
                WindowRc.RcRelease();
            }
        }
    }

    public Vector2<double> ScrollDelta => raw.ScrollDelta;
    public Vector2<double> CursorPositionDelta { get; private set; }

    private IReferenceCoutable WindowRc => Window;

    internal WindowInput(Window window) {
        Window = window;
        Window.Focused += (_, _) => resetCursorPositionDelta = true;
    }

    /// <summary>
    /// Returns <see cref="KeyState"/> from given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"><see cref="Key"/> to get the state of.</param>
    /// <returns>Current <see cref="KeyState"/> of <paramref name="key"/>.</returns>
    public KeyState GetKeyState(Key key) {
        return raw.GetKeyValue((int)key).State;
    }

    /// <summary>
    /// Returns <see cref="KeyState"/> and <paramref name="keyModifier"/> from given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"><see cref="Key"/> to get the state of.</param>
    /// <param name="keyModifier">Current <see cref="KeyModifier"/> of <paramref name="key"/>.</param>
    /// <returns>Current <see cref="KeyState"/> of <paramref name="key"/>.</returns>
    public KeyState GetKeyState(Key key, out KeyModifier keyModifier) {
        KeyValue value = raw.GetKeyValue((int)key);
        keyModifier = value.Modifier;
        return value.State;
    }

    /// <summary>
    /// Returns whether the <paramref name="key"/> is pressed.
    /// </summary>
    /// <param name="key"><see cref="Key"/> to get the state of.</param>
    /// <returns>
    /// Returns <see langword="true"/> if key state is <see cref="KeyState.JustPressed"/> or
    /// <see cref="KeyState.Pressed"/>; otherwise <see langword="false"/>.
    /// </returns>
    public bool Pressed(Key key) {
        return GetKeyState(key) >= KeyState.JustPressed;
    }

    /// <summary>
    /// Returns whether the <paramref name="key"/> is pressed.
    /// </summary>
    /// <param name="key"><see cref="Key"/> to get the state of.</param>
    /// <param name="keyModifier">Current <see cref="KeyModifier"/> of <paramref name="key"/>.</param>
    /// <returns>
    /// Returns <see langword="true"/> if key state is <see cref="KeyState.JustPressed"/> or
    /// <see cref="KeyState.Pressed"/>; otherwise <see langword="false"/>.
    /// </returns>
    public bool Pressed(Key key, out KeyModifier keyModifier) {
        return GetKeyState(key, out keyModifier) >= KeyState.JustPressed;
    }

    /// <summary>
    /// Returns whether the <paramref name="key"/> is pressed in this frame.
    /// </summary>
    /// <param name="key"><see cref="Key"/> to get the state of.</param>
    /// <returns>
    /// Returns <see langword="true"/> if key state is <see cref="KeyState.JustPressed"/>; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool JustPressed(Key key) {
        return GetKeyState(key) == KeyState.JustPressed;
    }

    /// <summary>
    /// Returns whether the <paramref name="key"/> is pressed in this frame.
    /// </summary>
    /// <param name="key"><see cref="Key"/> to get the state of.</param>
    /// <param name="keyModifier">Current <see cref="KeyModifier"/> of <paramref name="key"/>.</param>
    /// <returns>
    /// Returns <see langword="true"/> if key state is <see cref="KeyState.JustPressed"/> or; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool JustPressed(Key key, out KeyModifier keyModifier) {
        return GetKeyState(key, out keyModifier) == KeyState.JustPressed;
    }

    /// <summary>
    /// Returns whether the <paramref name="key"/> is released in this frame.
    /// </summary>
    /// <param name="key"><see cref="Key"/> to get the state of.</param>
    /// <returns>
    /// Returns <see langword="true"/> if key state is <see cref="KeyState.JustReleased"/>; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool JustReleased(Key key) {
        return GetKeyState(key) == KeyState.JustReleased;
    }

    /// <summary>
    /// Returns whether the <paramref name="key"/> is released in this frame.
    /// </summary>
    /// <param name="key"><see cref="Key"/> to get the state of.</param>
    /// <param name="keyModifier">Current <see cref="KeyModifier"/> of <paramref name="key"/>.</param>
    /// <returns>
    /// Returns <see langword="true"/> if key state is <see cref="KeyState.JustReleased"/> or; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool JustReleased(Key key, out KeyModifier keyModifier) {
        return GetKeyState(key, out keyModifier) == KeyState.JustPressed;
    }

    internal ref WindowInputRaw ProcessBeforePoolEvents() {
        lastCursorPosition = CursorPosition;
        return ref raw;
    }

    internal void ProcessAfterPoolEvents() {
        if (CursorLockMode == CursorLockMode.Locked && Window.IsFocused)
            CursorPosition = new Vector2<double>(Window.Width / 2, Window.Height / 2);
        else
            cursorPosition = raw.CursorPosition;

        if (resetCursorPositionDelta) {
            CursorPositionDelta = Vector2<double>.Zero;
            resetCursorPositionDelta = false;
        } else {
            CursorPositionDelta = raw.CursorPosition - lastCursorPosition;
        }
    }

}
