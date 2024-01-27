using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace YamNetUnity
{
    /// <summary>
    /// A YamNet-based classifier which identifies the types of sounds heard in a continuous audio stream.
    /// </summary>
    public class Classifier : IDisposable, ISampleReceiver
    {
        private readonly IWorker _worker;
        private readonly ITensorAllocator _tensorAllocator;
        private readonly Ops _tensorOps;
        private readonly AudioFeatureBuffer _featureBuffer;

        private readonly string _outputName;
        private readonly TensorFloat _input;
        private TensorFloat _output;
        
        public delegate void YamNetResultCallback(ReadOnlySpan<float> classScores);

        /// <summary>
        /// Event which is invoked when the classifier has classified a block of audio.
        /// The first parameter is the class ID, the second is the class name, and the third is the
        /// confidence level.
        /// </summary>
        public event YamNetResultCallback ResultReady;
        
        /// <summary>
        /// Creates a new YamNet audio classifier.
        /// </summary>
        /// <param name="modelAsset">The YamNet model to use.</param>
        public Classifier(ModelAsset modelAsset)
        {
            if (!modelAsset)
                throw new ArgumentNullException(nameof(modelAsset));

            var backendType = SystemInfo.supportsComputeShaders ? BackendType.GPUCompute : BackendType.CPU;
            
            var model = ModelLoader.Load(modelAsset);
            _tensorAllocator = new TensorCachingAllocator();
            _worker = WorkerFactory.CreateWorker(backendType, model);
            _tensorOps = WorkerFactory.CreateOps(backendType, _tensorAllocator);

            _featureBuffer = new AudioFeatureBuffer();
            
            var patchTensorShape = new TensorShape(1, 1, 96, 64);
            _worker.PrepareForInput(new Dictionary<string, TensorShape>(){{model.inputs[0].name, patchTensorShape}});
            
            _input = (TensorFloat)_tensorAllocator.Alloc(patchTensorShape, DataType.Float,
                new ArrayTensorData(patchTensorShape));

            _outputName = model.outputs[0];
        }
        
        public void AppendSamples(NativeSlice<float> waveform, int sampleRate)
        {
            if (sampleRate == AudioFeatureBuffer.InputSamplingRate)
            {
                AppendSamplesCorrectSampleRate(waveform);
            }
            else
            {
                // Resample the waveform to the required rate
                int resampledLength = (int)(waveform.Length * ((double)AudioFeatureBuffer.InputSamplingRate / sampleRate));
                float stepRate = (float)sampleRate / AudioFeatureBuffer.InputSamplingRate;
                var toWaveform = new NativeArray<float>(resampledLength, Allocator.TempJob);
                try
                {
                    for (int toIndex = 0; toIndex < toWaveform.Length; toIndex++)
                    {
                        int fromIndex = (int)(toIndex * stepRate);
                        if (fromIndex < waveform.Length)
                        {
                            toWaveform[toIndex] = waveform[fromIndex];
                        }
                    }

                    AppendSamplesCorrectSampleRate(toWaveform);
                }
                finally
                {
                    toWaveform.Dispose();
                }
            }
        }

        private void AppendSamplesCorrectSampleRate(NativeSlice<float> waveform)
        {
            Profiler.BeginSample(nameof(AppendSamplesCorrectSampleRate));
            int offset = 0;
            while (offset < waveform.Length)
            {
                int written = _featureBuffer.Write(waveform.Slice(offset));
                offset += written;
                while (_featureBuffer.OutputBuffer.Length >= 96 * 64)
                {
                    try
                    {
                        OnPatchReceived(_featureBuffer.OutputBuffer.Slice(0, 96 * 64));
                    }
                    finally
                    {
                        _featureBuffer.ConsumeOutput(48 * 64);
                    }
                }
            }       
            Profiler.EndSample();
        }
        
        private void OnPatchReceived(NativeSlice<float> features)
        {
            Profiler.BeginSample(nameof(OnPatchReceived));
            if (_output != null && !_output.IsAsyncReadbackRequestDone())
                _output.CompleteAllPendingOperations();
            
            unsafe
            {
                UnsafeUtility.MemCpy(((ArrayTensorData)_input.tensorOnDevice).array.RawPtr, 
                    features.GetUnsafeReadOnlyPtr(), 
                    sizeof(float) * features.Length);
            }
            
            _worker.Execute(_input);

            _output = _worker.PeekOutput(_outputName) as TensorFloat;
            _output.AsyncReadbackRequest(success =>
            {
                if (!success)
                    return;
                    
                _output.MakeReadable();
                _output.ToReadOnlySpan();
                ResultReady?.Invoke(_output.ToReadOnlySpan());
            });
            Profiler.EndSample();
        }


        public void Dispose()
        {
            _worker?.Dispose();
            _featureBuffer?.Dispose();
            _tensorOps?.Dispose();
            _tensorAllocator?.Dispose();
            _input?.Dispose();
            _output?.Dispose();
        }
    }
}