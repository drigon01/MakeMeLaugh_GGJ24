using System;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class MicrophoneWebGL
{
#if UNITY_WEBGL// && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void MicrophoneWebGL_Init(int bufferSize, int numberOfChannels);

    [DllImport("__Internal")]
    private static extern string MicrophoneWebGL_GetInitResult();

    [DllImport("__Internal")]
    private static extern void MicrophoneWebGL_Start();

    [DllImport("__Internal")]
    private static extern void MicrophoneWebGL_Stop();

    [DllImport("__Internal")]
    private static extern int MicrophoneWebGL_GetNumBuffers();

    [DllImport("__Internal")]
    private static extern unsafe bool MicrophoneWebGL_GetBuffer(void* bufferPtr);

    [DllImport("__Internal")]
    private static extern int MicrophoneWebGL_GetSampleRate();

    private static int _bufferSize;

    public static void Init(int bufferSize, int numberOfChannels)
    {
        _bufferSize = bufferSize;
        MicrophoneWebGL_Init(bufferSize, numberOfChannels);
    }

    public static string PollInit()
    {
        return MicrophoneWebGL_GetInitResult();
    }

    public static void Start()
    {
        MicrophoneWebGL_Start();
    }

    public static void Stop()
    {
        MicrophoneWebGL_Stop();
    }

    public static int GetNumBuffers()
    {
        return MicrophoneWebGL_GetNumBuffers();
    }

    public static bool GetBuffer(NativeSlice<float> buffer)
    {
        if (buffer.Length != _bufferSize)
            throw new ArgumentException(
                $"Incorrect buffer size {buffer.Length} - size at initialization was {_bufferSize}");

        unsafe
        {
            return MicrophoneWebGL_GetBuffer(buffer.GetUnsafePtr());
        }
    }

    public static int GetSampleRate()
    {
        return MicrophoneWebGL_GetSampleRate();
    }
#endif
}
