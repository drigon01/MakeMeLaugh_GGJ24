using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using YamNetUnity;

public class AnalysisDriver : MonoBehaviour
{
    public ModelAsset modelAsset;
    public ClassMap classMap;

    private Classifier _classifier;
    
    void Start()
    {
        _classifier = new Classifier(modelAsset, classMap);
        _classifier.ResultReady += OnClassifierResultReady;
        
#if UNITY_WEBGL
        var listener = gameObject.AddComponent<WebGLMicrophoneListener>();
        listener.SampleReceiver = _classifier;
#else
        var listener = gameObject.AddComponent<MicrophoneListener>();
        listener.SampleReceiver = _classifier;
#endif
    }

    private void OnClassifierResultReady(int bestclassid, string bestclassname, float bestscore)
    {
        Debug.Log(bestclassname);
    }

    void OnDestroy()
    {
        _classifier.Dispose();
    }
}
