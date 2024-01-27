using Unity.Collections;
using UnityEngine;

namespace YamNetUnity
{
    /// <summary>
    /// Component that listens to microphone input and sends it to an <see cref="ISampleReceiver"/>.
    /// </summary>
    public class MicrophoneListener : MonoBehaviour
    {
        public int AudioBufferLengthSec = 10;
        
        /// <summary>
        /// The <see cref="ISampleReceiver"/> that audio samples should be sent to.
        /// </summary>
        public ISampleReceiver SampleReceiver { get; set; }
        
        private AudioClip _clip;
        private string _microphoneDeviceName;
        private int _audioOffset;
        private int _sampleRate;

        private void Awake()
        {
            _microphoneDeviceName = null;
        }

        void OnEnable()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("No microphone device found");
                enabled = false;
                return;
            }
            
            _microphoneDeviceName = Microphone.devices[0];
            Microphone.GetDeviceCaps(_microphoneDeviceName, out var minFreq, out var maxFreq);
            
            _sampleRate = AudioFeatureBuffer.InputSamplingRate;
            if (minFreq != 0 && maxFreq != 0) 
                _sampleRate = Mathf.Clamp(_sampleRate, minFreq, maxFreq);

            _clip = Microphone.Start(_microphoneDeviceName, true, AudioBufferLengthSec, _sampleRate);
            _audioOffset = 0;
        }

        void Update()
        {
            int pos = Microphone.GetPosition(_microphoneDeviceName);
            if (pos < _audioOffset) 
                pos = _clip.samples;
            
            if (pos > _audioOffset && SampleReceiver != null)
            {
                float[] data = new float[pos - _audioOffset];
                _clip.GetData(data, _audioOffset);
                _audioOffset = pos;
                
                if (_audioOffset >= _clip.samples) 
                    _audioOffset = 0;

                using var nativeData = new NativeArray<float>(data, Allocator.TempJob);
                SampleReceiver.AppendSamples(nativeData, _sampleRate);
            }
        }

        void OnDisable()
        {
            Microphone.End(_microphoneDeviceName);
        }
    }
}
