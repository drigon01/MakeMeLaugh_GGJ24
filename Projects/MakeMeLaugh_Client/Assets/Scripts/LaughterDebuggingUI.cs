using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LaughterDebuggingUI : MonoBehaviour
{
    public AnalysisDriver analysisDriver;
    public UIDocument uiDocument;

    private List<Label> _classNames;
    private List<ProgressBar> _classProgressBars;
    private List<Label> _classScores;

    public void OnEnable()
    {
        _classNames = uiDocument.rootVisualElement.Query<Label>(className:"name").ToList();
        _classProgressBars = uiDocument.rootVisualElement.Query<ProgressBar>().ToList();
        _classScores = uiDocument.rootVisualElement.Query<Label>(className:"value").ToList();

        if (_classNames.Count != analysisDriver.classesOfInterest.Length)
            throw new Exception($"Wrong number of classes in UI - expected {analysisDriver.classesOfInterest.Length}, got {_classNames.Count}");

        for (int i = 0; i < _classNames.Count; ++i)
        {
            _classNames[i].text = analysisDriver.classesOfInterest[i];
            _classProgressBars[i].lowValue = 0;
            _classProgressBars[i].highValue = 1;
        }
    }

    public void Update()
    {
        for (int i = 0; i < _classNames.Count; ++i)
        {
            _classScores[i].text = analysisDriver.currentScoresOfInterest[i].ToString("0.00");
            _classProgressBars[i].value = analysisDriver.currentScoresOfInterest[i];
        }
        
        uiDocument.rootVisualElement.Q<Label>("maxClassName").text = analysisDriver.maxClassName;
        uiDocument.rootVisualElement.Q<Label>("maxClassValue").text = analysisDriver.maxClassScore.ToString("0.00");
    }
    
}
