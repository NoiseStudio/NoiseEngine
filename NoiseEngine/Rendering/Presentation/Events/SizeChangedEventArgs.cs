using System.ComponentModel;

namespace NoiseEngine.Rendering.Presentation.Events;

public class SizeChangedEventArgs : CancelEventArgs {

    public uint OldWidth { get; init; }
    public uint OldHeight { get; init; }
    public uint NewWidth { get; init; }
    public uint NewHeight { get; init; }

}
