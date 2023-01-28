using System;

namespace NoiseEngine.Inputs;

public class WindowInput {

    private readonly KeyValue[] keyValues = new KeyValue[1000];

    public Window Window { get; }

    internal WindowInput(Window window) {
        Window = window;
    }

    internal Span<KeyValue> GetKeyValueSpan() {
        return keyValues;
    }

}
