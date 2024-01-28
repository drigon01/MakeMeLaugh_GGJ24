using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;
using YamNetUnity;

public class AnalysisDriver : MonoBehaviour
{
    public ModelAsset modelAsset;
    public ClassMap classMap;

    private Classifier _classifier;

    public string[] classesOfInterest;
    [NonSerialized] public int[] classIDsOfInterest;
    public float[] currentScoresOfInterest;

    public string maxClassName;
    public int maxClassId;
    public float maxClassScore;
    
    void Start()
    {
        classIDsOfInterest = classesOfInterest.Select(className => classMap[className]).ToArray();
        currentScoresOfInterest = new float[classIDsOfInterest.Length];
        
        _classifier = new Classifier(modelAsset);
        _classifier.ResultReady += OnClassifierResultReady;
        
#if UNITY_WEBGL
        var listener = gameObject.AddComponent<WebGLMicrophoneListener>();
        listener.SampleReceiver = _classifier;
#else
        var listener = gameObject.AddComponent<MicrophoneListener>();
        listener.SampleReceiver = _classifier;
#endif
    }

    private void OnClassifierResultReady(ReadOnlySpan<float> classScores)
    {
        for (int i = 0; i < classIDsOfInterest.Length; ++i)
            currentScoresOfInterest[i] = classScores[i];

        maxClassScore = -1;
        for (int i = 0; i < classScores.Length; ++i)
        {
            if (classScores[i] > maxClassScore)
            {
                maxClassId = i;
                maxClassScore = classScores[i];
                maxClassName = classMap[i];
            }
        }
    }

    void OnDestroy()
    {
        _classifier.Dispose();
    }
}
