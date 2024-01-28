using System;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainUIViewModel : MonoBehaviour
{
  private const string DefaultIPAddress = "127.0.0.1";
  private const int DefaultPort = 7777;
  private const string DefaultPlayerName = "JerrySeinfeld";
  private VisualElement _rootElement;
  private VisualElement _popupHost;

  [SerializeField] private VisualTreeAsset _serverSettingsTemplate;
  [SerializeField] private VisualTreeAsset _waitingScreenTemplate;
  [SerializeField] private VisualTreeAsset _jokePunchlineTemplate;
  [SerializeField] private VisualTreeAsset _jokeSetupTemplate;
  [SerializeField] private StyleSheet _mainStyleSheet;

  [SerializeField] private ushort _port;
  [SerializeField] private string _ip;
  [SerializeField] private string _name;

  public static ConnectionManager ConnectionManager { get; private set; }

  private VisualElement _settingsView;
  private VisualElement _waitingScreen;
  private VisualElement _jokeEditor;
  private JokeEditorController _jokeEditController;
  private Button _connectButton;
  private VisualElement _setupEditor;
  private VisualElement _jokePunchline;

  [ContextMenu("TestSetup")]
  void TestSetup()
  {
    StartCoroutine(nameof(CreateJokesScreen));
    StartCoroutine(nameof(OnJokeSetupRequested), new PlayerSetupRequest("asd _BLANK_ asd", "312"));
  }

  [ContextMenu("TestPunchiles")]
  void TestPunchiles()
  {

    StartCoroutine(nameof(OnJokePunchlineRequested), new PlayerPunchlineRequest("setup", "whatever", "312"));
  }

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
    _jokeEditController = new JokeEditorController(_jokeEditor, _jokePunchlineTemplate, _jokeSetupTemplate);
    _jokeEditController.Done += OnDone;

    _rootElement.Add(_jokeEditor);
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

    _ip = PlayerPrefs.GetString("DefaultIPAddress", DefaultIPAddress);
    _port = (ushort)PlayerPrefs.GetInt("DefaultPort", DefaultPort);
    _name = PlayerPrefs.GetString("DefaultPlayerName", DefaultPlayerName);

    _connectButton = _settingsView.Q<Button>("Connect");
    var serverIP = _settingsView.Q<TextField>("IP");
    var serverPort = _settingsView.Q<TextField>("Port");
    var nameField = _settingsView.Q<TextField>("Name");

    _connectButton.clicked += OnConnectButtonClicked;

    serverIP.RegisterValueChangedCallback(OnIPChanged);
    serverPort.RegisterValueChangedCallback(OnPortChanged);
    nameField.RegisterValueChangedCallback(OnNameChanged);

    serverIP.value = _ip;
    serverPort.value = _port.ToString();
    nameField.value = _name;
  }

  private void SaveToPlayerPrefs()
  {
    PlayerPrefs.SetString("DefaultIPAddress", _ip);
    PlayerPrefs.SetInt("DefaultPort", _port);
    PlayerPrefs.SetString("DefaultPlayerName", _name);
  }

  private void OnPortChanged(ChangeEvent<string> evt)
  {
    _port = ushort.TryParse(evt.newValue, out var result) ? result : (ushort)0;
    _connectButton.SetEnabled(ValidateSettings());
  }

  private void OnNameChanged(ChangeEvent<string> evt)
  {
    _name = evt.newValue;
    _connectButton.SetEnabled(ValidateSettings());
  }

  private void OnIPChanged(ChangeEvent<string> evt)
  {
    _ip = evt.newValue;
    _connectButton.SetEnabled(ValidateSettings());
  }

  private bool ValidateSettings()
  {
    if (string.IsNullOrWhiteSpace(_ip))
      return false;

    if (_port == 0)
      return false;

    if (string.IsNullOrWhiteSpace(_name))
      return false;

    return true;
  }

  private void OnConnectButtonClicked()
  {
    if (!ValidateSettings())
      throw new InvalidOperationException("Invalid settings, can't connect");

    SaveToPlayerPrefs();

    if (ConnectionManager == null)
    {
      ConnectionManager = new ConnectionManager(_ip, _port, _name);
      ConnectionManager.Connected += OnConnectedToServer;
      ConnectionManager.JokeSetupRequested += OnJokeSetupRequested;
      ConnectionManager.JokePunchlineRequested += OnJokePunchlineRequested;
      ConnectionManager.SceneTransitionRequested += OnSceneTransitionRequested;
    }

    //should we validate befoore closing?
    ClosePopUp(_settingsView);

    //go to waiting popup
    var waitingInfo = new WaitingInfo("Waiting For Server", "....", "42%");

    UpdateWaitingScreeen(waitingInfo);
    ShowPopUp(_waitingScreen);
  }

  private void OnSceneTransitionRequested()
  {
    SceneManager.LoadScene("StageModeScene", LoadSceneMode.Additive);

    _popupHost.RemoveAt(0);
  }

  private void OnJokeSetupRequested(PlayerSetupRequest request)
  {
    ClosePopUp(_waitingScreen);
    _setupEditor = _jokeEditController.CreateSetupEditor(request);
    ShowPopUp(_setupEditor);
  }

  private void OnJokePunchlineRequested(PlayerPunchlineRequest request)
  {
    ClosePopUp(_setupEditor);
    _jokePunchline = _jokeEditController.CreatePunchlineEditor(request);


    ShowPopUp(_jokePunchline);
  }

  private void OnDone(MessageType type)
  {
    if (MessageType.PLAYER_PUNCHLINE_RESPONSE == type)
    {
      ClosePopUp(_jokePunchline);
    }
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
    var canRemove = _popupHost.Children().Contains(popupContent);

    if (canRemove)
    {
      _popupHost.Remove(popupContent);
    }
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
