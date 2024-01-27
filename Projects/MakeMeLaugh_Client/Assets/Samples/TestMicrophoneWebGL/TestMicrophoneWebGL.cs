using UnityEngine;
using System.Collections;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class TestMicrophoneWebGL : MonoBehaviour
{
    public Text InitResultText;
    public RectTransform RecordingDialog;

    private const int BufferSize = 16384;

    private AudioClip _clip;

    public IEnumerator Start()
    {
        MicrophoneWebGL.Init(BufferSize, 1);

        string initResult;
        do
        {
            initResult = MicrophoneWebGL.PollInit();
            InitResultText.text = initResult;
            yield return null;
        } while (initResult == "pending");

        if (initResult == "ready")
            RecordingDialog.gameObject.SetActive(true);
    }

    public void Record()
    {
        MicrophoneWebGL.Start();
    }

    public void Stop()
    {
        MicrophoneWebGL.Stop();

        var numBuffers = MicrophoneWebGL.GetNumBuffers();
        Debug.Log("recorded " + numBuffers + " buffers");
        Debug.Log("Sample rate is " + MicrophoneWebGL.GetSampleRate());

        var buffer = new NativeArray<float>(numBuffers * BufferSize, Allocator.Temp);
        for (int i = 0; i < numBuffers; ++i)
        {
            var ok = MicrophoneWebGL.GetBuffer(buffer.Slice(i * BufferSize, BufferSize));
            if (!ok)
                Debug.LogError("not ok");
        }

        _clip = AudioClip.Create("recordedclip", buffer.Length, 1, MicrophoneWebGL.GetSampleRate(), false);
        _clip.SetData(buffer.ToArray(), 0);

        buffer.Dispose();
    }

    public void Play()
    {
        var audioSource = GetComponent<AudioSource>();
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
