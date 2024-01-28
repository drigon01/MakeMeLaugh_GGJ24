﻿using System.Text.RegularExpressions;
using UnityEngine.UIElements;

public class JokeEditorController
{
  private VisualElement root;
  private readonly ConnectionManager connectionManager;
  private readonly VisualTreeAsset punchlineTemplate;
  private readonly VisualTreeAsset setupTemplate;

  public JokeEditorController(VisualElement root, ConnectionManager connectionManager, VisualTreeAsset punchlineTemplate, VisualTreeAsset setupTemplate)
  {
    this.root = root;
    this.connectionManager = connectionManager;
    this.punchlineTemplate = punchlineTemplate;
    this.setupTemplate = setupTemplate;
  }

  public void ShowPunchlineEditor(PlayerPunchlineRequest request)
  {
    punchlineTemplate.CloneTree(root);


  }

  Regex blankRegex = new Regex("(?<part1>.*)_BLANK_(?<part2>.*)");
  private string _jokeID;
  private Label _setupPart1;
  private Label _setupPart2;
  private TextField _setupBlank;

  public void ShowSetupEditor(PlayerSetupRequest request)
  {
    setupTemplate.CloneTree(root);
    var match= blankRegex.Match(request.SetupTemplate);

    _setupPart1 = root.Q<Label>("Setup_Part1");
    _setupPart2 = root.Q<Label>("Setup_Part2");
    _setupBlank = root.Q<TextField>();

    _setupPart1.text = match.Groups["part1"].Value;
    _setupPart2.text = match.Groups["part2"].Value;

    _jokeID = request.JokeId;


    var submit = root.Q<Button>();

    submit.clicked += OnSubmitSetup;

  }

  private void OnSubmitSetup()
  {
    var message = new PlayerSetupResponse($"{_setupPart1.text} {_setupBlank.text} {_setupPart2.text}",_jokeID) { };
    connectionManager.SendMessageToServer(message);
  }

  public void CloseEditor()
  {
    this.root = new VisualElement();
  }
}