using System.Runtime.InteropServices;
using NoiseEngine.Audio;

namespace NoiseEngine.Interop.Audio;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct AudioListenerEventHandlerRaw
{
    public readonly AudioListenerEventHandler.SampleHandlerDelegate SampleHandlerDelegate;

    public AudioListenerEventHandlerRaw(AudioListenerEventHandler.SampleHandlerDelegate sampleHandlerDelegate)
    {
        SampleHandlerDelegate = sampleHandlerDelegate;
    }
}
