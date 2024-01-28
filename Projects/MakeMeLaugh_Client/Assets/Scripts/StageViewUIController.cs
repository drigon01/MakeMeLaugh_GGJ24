using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class StageViewUIController : MonoBehaviour
{
    private Button _tomato;
    private Button _rose;
    private Button _rimshot;
    private Button _trumpet;

    private Label _laughterDetected;
    
    private AnalysisDriver _analysisDriver;
    
    public void Awake()
    {
        var doc = GetComponent<UIDocument>();
        
        _tomato = doc.rootVisualElement.Q<Button>("tomato");
        _rose = doc.rootVisualElement.Q<Button>("rose");
        _rimshot = doc.rootVisualElement.Q<Button>("rimshot");
        _trumpet = doc.rootVisualElement.Q<Button>("trumpet");
        
        _laughterDetected = doc.rootVisualElement.Q<Label>("laughter-detected");

        _analysisDriver = GetComponent<AnalysisDriver>();
    }
    
    public void OnEnable()
    {
        _tomato.clicked += OnTomatoClicked;
        _rose.clicked += OnRoseClicked;
        _rimshot.clicked += OnRimshotClicked;
        _trumpet.clicked += OnTrumpetClicked;

        _laughterDetected.style.visibility = Visibility.Hidden;
    }
    
    public void Update()
    {
        bool laughterDetected = _analysisDriver.classIDsOfInterest.Contains(_analysisDriver.maxClassId);
        _laughterDetected.style.visibility = laughterDetected ? Visibility.Visible : Visibility.Hidden;
    }

    private void OnTrumpetClicked()
    {
        
    }

    private void OnRimshotClicked()
    {
        
    }

    private void OnRoseClicked()
    {
        
    }

    private void OnTomatoClicked()
    {
        
    }

    public void OnDisable()
    {
        _tomato.clicked -= OnTomatoClicked;
        _rose.clicked -= OnRoseClicked;
        _rimshot.clicked -= OnRimshotClicked;
        _trumpet.clicked -= OnTrumpetClicked;

        _laughterDetected.style.visibility = Visibility.Hidden;
    }
}
