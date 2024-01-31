using System;
using System.Linq;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainUIViewModel : MonoBehaviour
{
  private const string DefaultIPAddress = "127.0.0.1";
  private const int DefaultPort = 7777;
  private const string DefaultPlayerName = "JerrySeinfeld";
  private string _joinCode;
  private UIDocument _uiDocument;
  private VisualElement _rootElement;
  private TemplateContainer _popupHost;

  [SerializeField] private VisualTreeAsset _serverSettingsTemplate;
  [SerializeField] private VisualTreeAsset _serverSettingsRelayTemplate;
  [SerializeField] private VisualTreeAsset _waitingScreenTemplate;
  [SerializeField] private VisualTreeAsset _jokePunchlineTemplate;
  [SerializeField] private VisualTreeAsset _jokeSetupTemplate;
  [SerializeField] private StyleSheet _mainStyleSheet;

  [SerializeField] private ushort _port;
  [SerializeField] private string _ip;
  [SerializeField] private string _name;
  [SerializeField] private bool _useRelay;

  [SerializeField] private ThemeStyleSheet _horizontalTheme;
  [SerializeField] private ThemeStyleSheet _verticalTheme;

  public static ConnectionManager ConnectionManager { get; private set; }

  public static event EventHandler LayoutChanged; 

  //these should probably be organised into separate custom controls
  private VisualElement _settingsView;
  private VisualElement _waitingScreen;
  private VisualElement _jokeEditor;
  private JokeEditorController _jokeEditController;
  private Button _connectButton;
  private VisualElement _setupEditor;
  private VisualElement _jokePunchline;

  private void Awake()
  {
    _uiDocument = GetComponent<UIDocument>();
    _rootElement = _uiDocument.rootVisualElement;
    _popupHost = new TemplateContainer() { name = "PopupHost" };

    _rootElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

    _popupHost.styleSheets.Add(_mainStyleSheet);
    _popupHost.AddToClassList("popup");

    _uiDocument.panelSettings.themeStyleSheet = _verticalTheme;

    _rootElement.panel.visualTree.Add(_popupHost);
  }

  private void OnDestroy()
  {
    _rootElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
  }

  private void OnGeometryChanged(GeometryChangedEvent evt)
  {
    if (evt.newRect.width > evt.newRect.height)
    {
      _uiDocument.panelSettings.themeStyleSheet = _horizontalTheme;
    }
    else 
    {
      _uiDocument.panelSettings.themeStyleSheet = _verticalTheme;
    }
  }

  private void Start()
  {
    //todo: create custom controls for each
    CreateServerSettings();
    CreateWaitingScreen();
    CreateJokesScreen();

    ShowPopUp(_settingsView);    
  }

  private void CreateJokesScreen()
  {
    if (_jokeEditController != null) return;

    _jokeEditor = new VisualElement();
    _jokeEditController = new JokeEditorController(_jokePunchlineTemplate, _jokeSetupTemplate);
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

    _name = PlayerPrefs.GetString("DefaultPlayerName", DefaultPlayerName);

    if (_useRelay)
    {
      if (!string.IsNullOrEmpty(Application.absoluteURL))
      {
        var query = new Uri(Application.absoluteURL).Query;
        var parameters = System.Web.HttpUtility.ParseQueryString(query);
        _joinCode = parameters["joinCode"];
      }

      _serverSettingsRelayTemplate.CloneTree(_settingsView);

      var joinCode = _settingsView.Q<TextField>("JoinCode");
      joinCode.RegisterValueChangedCallback(OnJoinCodeChanged);
      joinCode.value = _joinCode;
    }
    else
    {
      _ip = PlayerPrefs.GetString("DefaultIPAddress", DefaultIPAddress);
      _port = (ushort)PlayerPrefs.GetInt("DefaultPort", DefaultPort);

      _serverSettingsTemplate.CloneTree(_settingsView);

      var serverIP = _settingsView.Q<TextField>("IP");
      serverIP.RegisterValueChangedCallback(OnIPChanged);
      serverIP.value = _ip;

      var serverPort = _settingsView.Q<TextField>("Port");
      serverPort.RegisterValueChangedCallback(OnPortChanged);
      serverPort.value = _port.ToString();
    }

    var nameField = _settingsView.Q<TextField>("Name");
    nameField.RegisterValueChangedCallback(OnNameChanged);
    nameField.value = _name;

    _connectButton = _settingsView.Q<Button>("Connect");
    _connectButton.clicked += OnConnectButtonClicked;
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

  private void OnJoinCodeChanged(ChangeEvent<string> evt)
  {
    _joinCode = evt.newValue;
    _connectButton.SetEnabled(ValidateSettings());
  }

  private bool ValidateSettings()
  {
    if (_useRelay)
    {
      if (string.IsNullOrWhiteSpace(_joinCode))
        return false;
    }
    else
    {
      if (string.IsNullOrWhiteSpace(_ip))
        return false;

      if (_port == 0)
        return false;
    }

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
      if (_useRelay)
      {
        ConnectionManager = new ConnectionManager(_joinCode, _name);
      }
      else
      {
        ConnectionManager = new ConnectionManager(_ip, _port, _name);
      }

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
    ClosePopUp(_waitingScreen);
    _jokePunchline = _jokeEditController.CreatePunchlineEditor(request);
    ShowPopUp(_jokePunchline);
  }

  private void OnDone(MessageType type)
  {
    switch (type)
    {
      case MessageType.PLAYER_PUNCHLINE_RESPONSE:
        ClosePopUp(_jokePunchline);
        UpdateWaitingScreeen(new WaitingInfo("Waiting for a new punchline", "...", "65%"));
        ShowPopUp(_waitingScreen);
        break;

      case MessageType.PLAYER_SETUP_RESPONSE:
        ClosePopUp(_setupEditor);
        UpdateWaitingScreeen(new WaitingInfo("Waiting for a new punchline", "...", "34%"));
        ShowPopUp(_waitingScreen);
        break;
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
    description.text = info.SubText;
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

  #region Runtime Testing Functions

  [ContextMenu("TestSetup")]
  private void TestSetup()
  {
    StartCoroutine(nameof(Awake));
    StartCoroutine(nameof(CreateJokesScreen));
    StartCoroutine(nameof(OnJokeSetupRequested),
      new PlayerSetupRequest(
      "A long joke would _BLANK_ stay within bounds",
      "312"));
  }

  [ContextMenu("TestPunchiles")]
  private void TestPunchiles()
  {
    StartCoroutine(nameof(Awake));
    StartCoroutine(nameof(CreateJokesScreen));
    StartCoroutine(nameof(OnJokePunchlineRequested),
      new PlayerPunchlineRequest(
        "Setup for the punchline would be shown this way",
      "some words might be already here",
      "312"));
  }

  #endregion // Runtime Testing Functions
}
