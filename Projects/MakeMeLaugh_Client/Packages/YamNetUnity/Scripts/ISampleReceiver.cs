using Unity.Collections;

namespace YamNetUnity
{
    public interface ISampleReceiver
    {
        void AppendSamples(NativeSlice<float> samples, int sampleRate);
    }
}