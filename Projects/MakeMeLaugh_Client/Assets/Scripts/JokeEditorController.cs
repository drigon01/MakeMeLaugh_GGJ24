using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class JokeEditorController
{
  private VisualElement root;
  private readonly string uuid;
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

  public event Action DoneEditing;

  public JokeEditorController(VisualElement root, VisualTreeAsset punchlineTemplate, VisualTreeAsset setupTemplate)
  {
    this.root = root;
    this.uuid = uuid;
    this.connectionManager = MainUIViewModel.ConnectionManager;
    this.punchlineTemplate = punchlineTemplate;
    this.setupTemplate = setupTemplate;
  }

  public void ShowPunchlineEditor(PlayerPunchlineRequest request)
  {
    var punchlineEditor = new VisualElement() { name = "punchline" };
    punchlineTemplate.CloneTree(punchlineEditor);

    _fragments = punchlineEditor.Q<Label>("Fragments");
    _fragmentInput = punchlineEditor.Q<TextField>("Fragment");
    var submit = punchlineEditor.Q<Button>();

    _fragments.text = request.PunchlineTemplate;

    submit.clicked += OnSubmitPunchline;

    root.Add(punchlineEditor);
  }

  private void OnSubmitPunchline()
  {
    var message =
    new PlayerMessage(MainUIViewModel.ConnectionManager.ClientUUID, MessageType.PLAYER_PUNCHLINE_RESPONSE, JsonUtility.ToJson(new PlayerPunchlineResponse($"{_fragmentInput.text}", _jokeID)));
    MainUIViewModel.ConnectionManager.SendMessageToServer(message);

    if (root.Q("punchline") is VisualElement punchline)
    {
      root.Remove(punchline);
    }

    if (root.Q("setup") is VisualElement setup)
    {
      root.Remove(setup);
    }

    DoneEditing?.Invoke();
  }

  public void ShowSetupEditor(PlayerSetupRequest request)
  {

    var setupEditor = new VisualElement() { name = "setup" };

    setupTemplate.CloneTree(root);
    var match = blankRegex.Match(request.SetupTemplate);

    _setupPart1 = root.Q<Label>("Setup_Part1");
    _setupPart2 = root.Q<Label>("Setup_Part2");
    _setupBlank = root.Q<TextField>();

    _setupPart1.text = match.Groups["part1"].Value;
    _setupPart2.text = match.Groups["part2"].Value;

    _jokeID = request.JokeId;

    var submit = root.Q<Button>();
    submit.clicked += OnSubmitSetup;

    root.Add(setupEditor);
  }

  private void OnSubmitSetup()
  {

    var message =
 new PlayerMessage(MainUIViewModel.ConnectionManager.ClientUUID, MessageType.PLAYER_SETUP_RESPONSE, JsonUtility.ToJson(new PlayerSetupResponse($"{_setupPart1.text} {_setupBlank.text} {_setupPart2.text}", _jokeID)));
    MainUIViewModel.ConnectionManager.SendMessageToServer(message);

    if (root.Q("punchline") is VisualElement punchline)
    {
      root.Remove(punchline);
    }

    if (root.Q("setup") is VisualElement setup)
    {
      root.Remove(setup);
    }

    DoneEditing?.Invoke();
  }


  public void CloseEditor()
  {
    this.root = new VisualElement();
  }
}