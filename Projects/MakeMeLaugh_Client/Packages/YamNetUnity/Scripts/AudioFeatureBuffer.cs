using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace YamNetUnity
{
    internal class AudioFeatureBuffer : IDisposable
    {
        public const int InputSamplingRate = 16000;

        private readonly MelSpectrogram _processor;
        private readonly int _stftHopLength;
        private readonly int _stftWindowLength;
        private readonly int _nMelBands;

        private readonly NativeArray<float> _waveformBuffer;
        private int _waveformCount;
        private readonly NativeArray<float> _outputBuffer;
        private int _outputCount;

        public AudioFeatureBuffer(int stftHopLength = 160, int stftWindowLength = 400, int nMelBands = 64)
        {
            _processor = new MelSpectrogram();
            _stftHopLength = stftHopLength;
            _stftWindowLength = stftWindowLength;
            _nMelBands = nMelBands;

            _waveformBuffer = new NativeArray<float>(2 * _stftHopLength + _stftWindowLength, Allocator.Persistent);
            _waveformCount = 0;
            _outputBuffer = new NativeArray<float>(_nMelBands * (_stftWindowLength + _stftHopLength), Allocator.Persistent);
            _outputCount = 0;
        }

        public NativeSlice<float> OutputBuffer => _outputBuffer.Slice(0, _outputCount);

        public int Write(NativeSlice<float> waveform)
        {
            int written = 0;

            if (_waveformCount > 0)
            {
                int needed = ((_waveformCount - 1) / _stftHopLength) * _stftHopLength + _stftWindowLength - _waveformCount;
                written = Math.Min(needed, waveform.Length);

                var sourceSlice = waveform.Slice(0, written);
                var destSlice = _waveformBuffer.Slice(_waveformCount, written);
                destSlice.CopyFrom(sourceSlice);
                _waveformCount += written;

                int wavebufferOffset = 0;
                while (wavebufferOffset + _stftWindowLength < _waveformCount)
                {
                    _processor.Transform(_waveformBuffer.Slice(wavebufferOffset), _outputBuffer.Slice(_outputCount));
                    _outputCount += _nMelBands;
                    wavebufferOffset += _stftHopLength;
                }

                if (written < needed)
                {
                    unsafe
                    {
                        float* buf = (float*)_waveformBuffer.GetUnsafePtr();
                        UnsafeUtility.MemMove(buf,
                            buf + wavebufferOffset, _waveformCount - wavebufferOffset);
                    }

                    _waveformCount -= wavebufferOffset;
                    return written;
                }

                _waveformCount = 0;
                written -= _stftWindowLength - _stftHopLength;
            }

            while (written + _stftWindowLength < waveform.Length)
            {
                if (_outputCount + _nMelBands >= _outputBuffer.Length)
                {
                    return written;
                }
                _processor.Transform(waveform.Slice(written), _outputBuffer.Slice(_outputCount));
                _outputCount += _nMelBands;
                written += _stftHopLength;
            }

            var sourceSlice2 = waveform.Slice(written, waveform.Length - written);
            var destSlice2 = _waveformBuffer.Slice(0, waveform.Length - written);
            destSlice2.CopyFrom(sourceSlice2);
            _waveformCount = waveform.Length - written;
            written = waveform.Length;
            return written;
        }

        public unsafe void ConsumeOutput(int count)
        {
            float* buf = (float*)_outputBuffer.GetUnsafePtr();
            UnsafeUtility.MemMove(buf, buf + count, (_outputCount - count) * sizeof(float));
            _outputCount -= count;
        }

        public void Dispose()
        {
            _outputBuffer.Dispose();
            _processor.Dispose();
            _waveformBuffer.Dispose();
        }
    }
}
