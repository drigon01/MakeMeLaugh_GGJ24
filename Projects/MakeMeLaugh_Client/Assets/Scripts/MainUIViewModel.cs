using System;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

public class MainUIViewModel : MonoBehaviour
{
  private VisualElement _rootElement;

  [SerializeField] private VisualTreeAsset _serverSettingsTemplate;
  [SerializeField] private ushort _port;
  [SerializeField] private string _ip;

  ConnectionManager _connectionManager;

  private VisualElement settingsView;

  // Start is called before the first frame update
  private void Awake()
  {
    var document = GetComponent<UIDocument>();
    _rootElement = document.rootVisualElement;
  }

  private void Start()
  {
    settingsView = new VisualElement();
    _serverSettingsTemplate.CloneTree(settingsView);

    var connectButton = settingsView.Q<Button>("Connect");
    var serverIP = settingsView.Q<TextField>("IP");
    var serverPort = settingsView.Q<TextField>("Port");

    connectButton.clicked += OnConnectButtonClicked;

    serverIP.value = "127.0.0.1";
    serverPort.value = "7771";

    serverIP.RegisterValueChangedCallback(OnIPChanged);
    serverPort.RegisterValueChangedCallback(OnPortChanged);

    ShowPopUp(settingsView);
  }

  private void OnPortChanged(ChangeEvent<string> evt)
  {
    if (ushort.TryParse(evt.newValue, out var result))
    {
      _port = result;
    }
    else
    {
      throw new ArgumentException("Incccorrect value provided as port", "Port");
    }
  }

  private void OnIPChanged(ChangeEvent<string> evt)
  {
    if (!string.IsNullOrWhiteSpace(evt.newValue))
    {
      _ip = evt.newValue;
    }
    else
    {
      throw new ArgumentException("Incccorrect value provided for ip", nameof(IPAddress));
    }
  }

  private void OnConnectButtonClicked()
  {
    if (_connectionManager == null) { 
      _connectionManager = new ConnectionManager(_ip,_port);
    }

    //TODO: add actual connection logic
    ClosePopUp(settingsView);
  }


  private void Update()
  {
    if (_connectionManager != null)
    {
      _connectionManager.ExecuteUpdate();
    }
  }

  private void ShowPopUp(VisualElement popup)
  {
    popup.AddToClassList("popup");
    _rootElement.Insert(0, popup);
  }

  private void ClosePopUp(VisualElement popup)
  {
    _rootElement.Remove(popup);
  }
}
