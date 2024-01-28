using System;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

public class MainUIViewModel : MonoBehaviour
{
  private VisualElement _rootElement;
  private VisualElement _popupHost;
  [SerializeField] private VisualTreeAsset _serverSettingsTemplate;
  [SerializeField] private VisualTreeAsset _waitingScreenTemplate;
  [SerializeField] private StyleSheet _mainStyleSheet;

  [SerializeField] private ushort _port;
  [SerializeField] private string _ip;

  public static ConnectionManager ConnectionManager { get; private set; }

  private VisualElement _settingsView;
  private VisualElement _waitingScreen;

  // Start is called before the first frame update
  private void Awake()
  {
    var document = GetComponent<UIDocument>();
    _rootElement = document.rootVisualElement;
    _popupHost = new VisualElement() { name = "PopupHost" };

    _popupHost.styleSheets.Add(_mainStyleSheet);
    _popupHost.AddToClassList("popup");

    _rootElement.panel.visualTree.Add(_popupHost);
  }

  private void Start()
  {
    CreateServerSettings();
    CreateWaitingScreen();

    ShowPopUp(_settingsView);
  }

  private void CreateWaitingScreen()
  {
    _waitingScreen = new VisualElement();
    _waitingScreenTemplate.CloneTree(_waitingScreen);
  }

  private void CreateServerSettings()
  {
    _settingsView = new VisualElement();
    _serverSettingsTemplate.CloneTree(_settingsView);

    var connectButton = _settingsView.Q<Button>("Connect");
    var serverIP = _settingsView.Q<TextField>("IP");
    var serverPort = _settingsView.Q<TextField>("Port");

    connectButton.clicked += OnConnectButtonClicked;

    serverIP.value = "127.0.0.1";
    serverPort.value = "7777";

    _ip = serverIP.value;
    _port = ushort.Parse(serverPort.value);

    serverIP.RegisterValueChangedCallback(OnIPChanged);
    serverPort.RegisterValueChangedCallback(OnPortChanged);
  }

  private void OnPortChanged(ChangeEvent<string> evt)
  {
    if (ushort.TryParse(evt.newValue, out var result))
    {
      _port = result;
    }
    else
    {
      throw new ArgumentException("Incorrect value provided as port", "Port");
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
      throw new ArgumentException("Incorrect value provided for ip", nameof(IPAddress));
    }
  }

  private void OnConnectButtonClicked()
  {

    if (ConnectionManager == null)
    {
       ConnectionManager = new ConnectionManager(_ip,_port);
    }

    //should we validate befoore closing?
    ClosePopUp(_settingsView);

    //go to waiting popup
    var waitingInfo = new WaitingInfo("TEST", "asdas", "42%");

    UpdateWaitingScreeen(waitingInfo);

    ShowPopUp(_waitingScreen);
  }

  private void UpdateWaitingScreeen(WaitingInfo info)
  {
    var title = _waitingScreen.Q<Label>("Title");
    var description = _waitingScreen.Q<Label>("Description");
    var percentage = _waitingScreen.Q<Label>("PercentIndicator");


    title.text = info.Title;
    description.text = info.Text;
    percentage.text = info.Percent;

  }

  private void Update()
  {
    if (ConnectionManager != null)
    {
      ConnectionManager.ExecuteUpdate();
    }
  }

  private void ShowPopUp(VisualElement popupContent)
  {
    _popupHost.Insert(0, popupContent);
    
  }

  private void ClosePopUp(VisualElement popupContent)
  {
    _popupHost.Remove(popupContent);
  }
}