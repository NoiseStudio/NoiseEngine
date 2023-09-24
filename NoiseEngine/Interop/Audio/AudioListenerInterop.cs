namespace NoiseEngine.Interop.Audio;

internal static partial class AudioListenerInterop
{
    [InteropImport("audio_audio_listener_create")]
    public static partial void Create(ulong id);
}
