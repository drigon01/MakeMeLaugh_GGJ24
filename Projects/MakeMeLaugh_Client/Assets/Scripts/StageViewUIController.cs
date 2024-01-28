using System;
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

        _laughterDetected.SetEnabled(false);
    }

    public float CooldownBetweenLaughterDetections = 0.5f;
    private float _lastLaughterAt;
    
    public void Update()
    {
        bool laughterDetected = _analysisDriver.classIDsOfInterest.Contains(_analysisDriver.maxClassId);
        if (laughterDetected && (_lastLaughterAt + CooldownBetweenLaughterDetections < Time.time) && MainUIViewModel.ConnectionManager != null)
        {
            var message = new PlayerMessage(PlayerUUID, MessageType.PLAYER_LAUGHED);
            MainUIViewModel.ConnectionManager.SendMessageToServer(message);
        }
        _laughterDetected.SetEnabled(laughterDetected);
    }

    private string PlayerUUID => MainUIViewModel.ConnectionManager.ClientUUID;
    
    private void OnTomatoClicked()
    {
        var message = new PlayerMessage(PlayerUUID, MessageType.PLAYER_DEPLOY_TOMATO);
        MainUIViewModel.ConnectionManager.SendMessageToServer(message);
    }

    private void OnRoseClicked()
    {
        var message = new PlayerMessage(PlayerUUID, MessageType.PLAYER_DEPLOY_ROSE);
        MainUIViewModel.ConnectionManager.SendMessageToServer(message);
    }
    
    private void OnRimshotClicked()
    {
        var message = new PlayerMessage(PlayerUUID, MessageType.PLAYER_DEPLOY_RIMSHOT);
        MainUIViewModel.ConnectionManager.SendMessageToServer(message);
    }
    
    private void OnTrumpetClicked()
    {
        var message = new PlayerMessage(PlayerUUID, MessageType.PLAYER_DEPLOY_TRUMPET);
        MainUIViewModel.ConnectionManager.SendMessageToServer(message);
    }

    public void OnDisable()
    {
        _tomato.clicked -= OnTomatoClicked;
        _rose.clicked -= OnRoseClicked;
        _rimshot.clicked -= OnRimshotClicked;
        _trumpet.clicked -= OnTrumpetClicked;

        _laughterDetected.SetEnabled(false);
    }
}
