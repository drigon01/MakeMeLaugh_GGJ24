using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class JokeEditorController
{
  private readonly ConnectionManager connectionManager;
  private readonly VisualTreeAsset punchlineTemplate;
  private readonly VisualTreeAsset setupTemplate;
  private readonly Regex blankRegex = new Regex("(?<part1>.*)_BLANK_(?<part2>.*)");

  private string _jokeID;
  private Label _setupPart1;
  private Label _fragments;
  private TextField _fragmentInput;
  private Label _setupPart2;
  private TextField _setupBlank;
  private VisualElement _punchlineEditor;
  private VisualElement _setupEditor;


  public event Action<MessageType> Done;

  public JokeEditorController(VisualElement root, VisualTreeAsset punchlineTemplate, VisualTreeAsset setupTemplate)
  {
    this.connectionManager = MainUIViewModel.ConnectionManager;
    this.punchlineTemplate = punchlineTemplate;
    this.setupTemplate = setupTemplate;
  }

  public VisualElement CreatePunchlineEditor(PlayerPunchlineRequest request)
  {
    _punchlineEditor = new VisualElement() { name = "punchline" };
    punchlineTemplate.CloneTree(_punchlineEditor);

    _fragments = _punchlineEditor.Q<Label>("Fragments");
    _fragmentInput = _punchlineEditor.Q<TextField>("Fragment");
    var submit = _punchlineEditor.Q<Button>();

    _fragments.text = request.PunchlineTemplate;

    submit.clicked += OnSubmitPunchline;

    return _punchlineEditor;
  }

  private void OnSubmitPunchline()
  {
    var message =
    new PlayerMessage(MainUIViewModel.ConnectionManager.ClientUUID, MessageType.PLAYER_PUNCHLINE_RESPONSE, JsonUtility.ToJson(new PlayerPunchlineResponse($"{_fragmentInput.text}", _jokeID)));
    MainUIViewModel.ConnectionManager.SendMessageToServer(message);
    Done?.Invoke(MessageType.PLAYER_PUNCHLINE_RESPONSE);


  }

  public VisualElement CreateSetupEditor(PlayerSetupRequest request)
  {

    _setupEditor = new VisualElement() { name = "setup" };

    setupTemplate.CloneTree(_setupEditor);
    var match = blankRegex.Match(request.SetupTemplate);

    _setupPart1 = _setupEditor.Q<Label>("Setup_Part1");
    _setupPart2 = _setupEditor.Q<Label>("Setup_Part2");
    _setupBlank = _setupEditor.Q<TextField>();

    _setupPart1.text = match.Groups["part1"].Value;
    _setupPart2.text = match.Groups["part2"].Value;

    _jokeID = request.JokeId;

    var submit = _setupEditor.Q<Button>();
    submit.clicked += OnSubmitSetup;

    return _setupEditor;
  }

  private void OnSubmitSetup()
  {

    var message =
 new PlayerMessage(MainUIViewModel.ConnectionManager.ClientUUID, MessageType.PLAYER_SETUP_RESPONSE, JsonUtility.ToJson(new PlayerSetupResponse($"{_setupPart1.text} {_setupBlank.text} {_setupPart2.text}", _jokeID)));
    MainUIViewModel.ConnectionManager.SendMessageToServer(message);

    Done?.Invoke(MessageType.PLAYER_SETUP_RESPONSE);
  }
}