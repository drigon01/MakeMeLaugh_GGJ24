

using Unity.Collections;
using UnityEngine;
using YamNetUnity;

public class WebGLMicrophoneListener : MonoBehaviour
{
    public int BufferSize = 1024;
    public ISampleReceiver SampleReceiver;

    private NativeArray<float> _buffer;
    private int? _sampleRate;
    
    public void OnEnable()
    {
        _buffer = new NativeArray<float>(BufferSize, Allocator.Persistent);
        
        MicrophoneWebGL.Init(BufferSize, 1);
        MicrophoneWebGL.Start();
    }

    public void Update()
    {
        if (MicrophoneWebGL.PollInit() != "ready")
            return;

        _sampleRate ??= MicrophoneWebGL.GetSampleRate();

        while (MicrophoneWebGL.GetBuffer(_buffer))
            SampleReceiver.AppendSamples(_buffer, _sampleRate.Value);
    }

    public void OnDisable()
    {
        MicrophoneWebGL.Stop();
        _buffer.Dispose();
        _sampleRate = null;
    }
}
