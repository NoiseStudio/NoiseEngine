using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using NoiseEngine.Interop;
using NoiseEngine.Interop.Audio;

namespace NoiseEngine.Audio;

internal static class AudioListenerEventHandler
{
    private static readonly ConcurrentDictionary<ulong, AudioListener> listeners =
        new ConcurrentDictionary<ulong, AudioListener>();

    private static readonly AudioListenerEventHandlerRaw raw;
    
    static AudioListenerEventHandler()
    {
        raw = new AudioListenerEventHandlerRaw(SampleHandler);
        // if (!AudioListenerEventHandlerInterop.Initialize(raw).TryGetValue(out _, out ResultError error))
        //     error.ThrowAndDispose();
    }
    
    [UnmanagedFunctionPointer(InteropConstants.CallingConvention)]
    public delegate void SampleHandlerDelegate(ulong id, float[] sampleBuffer);

    public static void RegisterListener(AudioListener listener)
    {
        listeners.TryAdd(listener.Id, listener);
    }
    
    private static void SampleHandler(ulong id, float[] sampleBuffer)
    {
        listeners[id].SampleHandler(sampleBuffer);
    }
}
