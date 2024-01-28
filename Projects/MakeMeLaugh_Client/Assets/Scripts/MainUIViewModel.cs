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
  [SerializeField] private VisualTreeAsset _jokeEditorTemplate;
  [SerializeField] private StyleSheet _mainStyleSheet;

  [SerializeField] private ushort _port;
  [SerializeField] private string _ip;
  [SerializeField] private string _name;
  
  public static ConnectionManager ConnectionManager { get; private set; }

  private VisualElement _settingsView;
  private VisualElement _waitingScreen;
  private VisualElement _jokeEditor;

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
    CreateJokesScreen();

    ShowPopUp(_settingsView);
  }

  private void CreateJokesScreen()
  {
    _jokeEditor = new VisualElement();
    _jokeEditorTemplate.CloneTree(_jokeEditor);
  }

  private void CreateWaitingScreen()
  {
    _waitingScreen = new VisualElement();
    _waitingScreenTemplate.CloneTree(_waitingScreen);

    SetupLoadingAnimation(_waitingScreen);
  }

  private void CreateServerSettings()
  {
    _settingsView = new VisualElement();
    _serverSettingsTemplate.CloneTree(_settingsView);

    var connectButton = _settingsView.Q<Button>("Connect");
    var serverIP = _settingsView.Q<TextField>("IP");
    var serverPort = _settingsView.Q<TextField>("Port");
    var name = _settingsView.Q<TextField>("Name");

    connectButton.clicked += OnConnectButtonClicked;

    serverIP.RegisterValueChangedCallback(OnIPChanged);
    serverPort.RegisterValueChangedCallback(OnPortChanged);
    name.RegisterValueChangedCallback(OnNameChanged);

    _ip = "127.0.0.1";
    _port = ushort.Parse("7777");
    _name = "Joe";

    serverIP.value = _ip;
    serverPort.value = _port.ToString();
    name.value = _name;
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

  private void OnNameChanged(ChangeEvent<string> evt)
  {
    if (!string.IsNullOrWhiteSpace(evt.newValue))
    {
      _name = evt.newValue;
    }
    else
    {
      throw new ArgumentException("Missing name");
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
      ConnectionManager = new ConnectionManager(_ip, _port, _name);
      ConnectionManager.Connected += OnConnectedToServer;
      ConnectionManager.JokesReceived += OnJokeReceived;
    }

    //should we validate befoore closing?
    ClosePopUp(_settingsView);

    //go to waiting popup
    var waitingInfo = new WaitingInfo("Waiting For Server", "....", "42%");

    UpdateWaitingScreeen(waitingInfo);
    ShowPopUp(_waitingScreen);
  }

  private void OnJokeReceived(JokeTempalte obj)
  {
    ClosePopUp(_waitingScreen);
    UpdateJokes();

    ShowPopUp(_jokeEditor);
  }

  private void UpdateJokes()
  {
     //Template logic to update the jokes go here

  }

  private void OnConnectedToServer()
  {
    var waitingInfo = new WaitingInfo("Waiting For Players to Join", "....", "69%");
    UpdateWaitingScreeen(waitingInfo);
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

  private void SetupLoadingAnimation(VisualElement root)
  {
    var indicator = root.Q<VisualElement>("LoadingIndicator");
    // When the animation ends, the callback toggles a class to rotate
    indicator.RegisterCallback<TransitionEndEvent>(evt => indicator.ToggleInClassList("rotate-indicator"));
    // Schedule the first transition 100 milliseconds after the root.schedule.Execute method is called.
    root.schedule.Execute(() => indicator.ToggleInClassList("rotate-indicator")).StartingIn(100);
  }
}
