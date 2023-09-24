using System.Diagnostics;
using System.Threading;
using NoiseEngine.Interop.Audio;

namespace NoiseEngine.Audio;

internal class AudioListener
{
    private static ulong nextId;
    public readonly ulong Id;

    public AudioListener()
    {
        Id = nextId;
        nextId++;
        //AudioListenerEventHandler.RegisterListener(this);
        AudioListenerInterop.Create(Id);
        //Thread.Sleep(1000);
    }
    
    public void SampleHandler(float[] sampleBuffer)
    {
        Log.Debug("yessss");
        Debug.Assert(false);
    }
}
