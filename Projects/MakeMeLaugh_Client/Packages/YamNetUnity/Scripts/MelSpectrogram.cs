using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace YamNetUnity
{
    internal class MelSpectrogram : IDisposable
    {
        private NativeArray<double> _window;
        private NativeArray<double> _melBands;
        private readonly int _fftLength;
        private readonly int _nMelBands;
        private readonly double _sampleRate;
        private readonly double _logOffset;

        public MelSpectrogram(
            int sampleRate = 16000,
            int stftWindowLength = 400, int stftLength = 512,
            int nMelBands = 64, double melMinHz = 125.0, double melMaxHz = 7500.0,
            double logOffset = 0.001)
        {
            _window = MakeHannWindow(stftWindowLength);
            _melBands = MakeMelBands(melMinHz, melMaxHz, nMelBands);
            
            _fftLength = stftLength;
            _nMelBands = nMelBands;
            _sampleRate = sampleRate;
            _logOffset = logOffset;
        }

        static NativeArray<double> MakeHannWindow(int windowLength)
        {
            var window = new NativeArray<double>(windowLength, Allocator.Persistent);
            for (int i = 0; i < windowLength; i++)
            {
                window[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / windowLength));
            }
            return window;
        }
        
        [BurstCompile]
        private struct DoTransformJob : IJob
        {
            [ReadOnly] public NativeSlice<float> waveform;
            [WriteOnly] public NativeSlice<float> melspec;

            [ReadOnly] public NativeArray<double> window;
            [ReadOnly] public NativeArray<double> melBands;
            public int _fftLength;
            public int _nMelBands;
            public double _sampleRate;
            public double _logOffset;
            
            public void Execute()
            {
                using var temp1 = new NativeArray<double>(_fftLength, Allocator.Temp);
                using var temp2 = new NativeArray<double>(_fftLength, Allocator.Temp);
            
                GetFrame(waveform, temp1, window);
                CFFT(temp1, temp2, _fftLength);
                ToMagnitude(temp2, temp1, _fftLength);
                ToMelSpec(temp2, melspec, melBands, _nMelBands, _fftLength, _sampleRate, _logOffset);
            }
        }

        public void Transform(NativeSlice<float> waveform, NativeSlice<float> melspec)
        {
            ScheduleTransform(waveform, melspec).Complete();
        }

        public JobHandle ScheduleTransform(NativeSlice<float> waveform, NativeSlice<float> melspec)
        {
            var jobHandle = new DoTransformJob()
            {
                waveform = waveform,
                melspec = melspec,

                window = _window,
                melBands = _melBands,

                _fftLength = _fftLength,
                _nMelBands = _nMelBands,
                _sampleRate = _sampleRate,
                _logOffset = _logOffset
            }.Schedule();
            return jobHandle;
        }

        private static void ToMelSpec(ReadOnlySpan<double> spec, NativeSlice<float> melspec, 
            ReadOnlySpan<double> melBands, int nMelBands, int fftLength, double sampleRate, double logOffset)
        {
            for (int i = 0; i < nMelBands; i++)
            {
                double startHz = melBands[i];
                double peakHz = melBands[i + 1];
                double endHz = melBands[i + 2];
                double v = 0.0;
                int j = (int)(startHz * fftLength / sampleRate) + 1;
                while (true)
                {
                    double hz = j * sampleRate / fftLength;
                    if (hz > peakHz)
                        break;
                    double r = (hz - startHz) / (peakHz - startHz);
                    v += spec[j] * r;
                    j++;
                }
                while (true)
                {
                    double hz = j * sampleRate / fftLength;
                    if (hz > endHz)
                        break;
                    double r = (endHz - hz) / (endHz - peakHz);
                    v += spec[j] * r;
                    j++;
                }
                melspec[i] = (float)Math.Log(v + logOffset);
            }
        }

        private static void GetFrame(NativeSlice<float> waveform, Span<double> frame, NativeArray<double> window)
        {
            for (int i = 0; i < window.Length; i++)
            {
                frame[i] = waveform[i] * window[i];
            }
            for (int i = window.Length; i < frame.Length; i++)
            {
                frame[i] = 0.0;
            }
        }

        private static void ToMagnitude(Span<double> xr, ReadOnlySpan<double> xi, int N)
        {
            for (int n = 0; n < N; n++)
            {
                xr[n] = Math.Sqrt(xr[n] * xr[n] + xi[n] * xi[n]);
            }
        }

        private static double HzToMel(double hz) => 2595 * Math.Log10(1 + hz / 700);

        static double MelToHz(double mel) => (Math.Pow(10, mel / 2595) - 1) * 700;

        static NativeArray<double> MakeMelBands(double melMinHz, double melMaxHz, int nMelBanks)
        {
            double melMin = HzToMel(melMinHz);
            double melMax = HzToMel(melMaxHz);
            var melBanks = new NativeArray<double>(nMelBanks + 2, Allocator.Persistent);
            for (int i = 0; i < nMelBanks + 2; i++)
            {
                double mel = (melMax - melMin) * i / (nMelBanks + 1) + melMin;
                melBanks[i] = MelToHz(mel);
            }
            return melBanks;
        }

        static int SwapIndex(int i)
        {
            return (i >> 8) & 0x01
                 | (i >> 6) & 0x02
                 | (i >> 4) & 0x04
                 | (i >> 2) & 0x08
                 | (i) & 0x10
                 | (i << 2) & 0x20
                 | (i << 4) & 0x40
                 | (i << 6) & 0x80
                 | (i << 8) & 0x100;
        }

        private static void CFFT(Span<double> xr, Span<double> xi, int N)
        {
            var t = xi;
            xi = xr;
            xr = t;
            for (int i = 0; i < N; i++)
            {
                xr[i] = xi[SwapIndex(i)];
            }
            for (int i = 0; i < N; i++)
            {
                xi[i] = 0.0;
            }
            for (int n = 1; n < N; n *= 2)
            {
                for (int j = 0; j < N; j += n * 2)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double ar = Math.Cos(-Math.PI * k / n);
                        double ai = Math.Sin(-Math.PI * k / n);
                        double er = xr[j + k];
                        double ei = xi[j + k];
                        double or = xr[j + k + n];
                        double oi = xi[j + k + n];
                        double aor = ar * or - ai * oi;
                        double aoi = ai * or + ar * oi;
                        xr[j + k] = er + aor;
                        xi[j + k] = ei + aoi;
                        xr[j + k + n] = er - aor;
                        xi[j + k + n] = ei - aoi;
                    }
                }
            }
        }

        private static void CFFTRef(double[] xr, double[] xi, int N)
        {
            double[] yr = new double[N];
            double[] yi = new double[N];
            for (int i = 0; i < N; i++)
            {
                double vr = 0.0;
                double vi = 0.0;
                for (int k = 0; k < N; k++)
                {
                    vr += Math.Cos(-2 * Math.PI * k * i / N) * xr[k];
                    vi += Math.Sin(-2 * Math.PI * k * i / N) * xr[k];
                }
                yr[i] = vr;
                yi[i] = vi;
            }
            for (int i = 0; i < N; i++)
            {
                xr[i] = yr[i];
                xi[i] = yi[i];
            }
        }

        public void Dispose()
        {
            _window.Dispose();
            _melBands.Dispose();
        }
    }
}
