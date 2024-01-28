public struct PlayerPunchlineRequest
{
  public PlayerPunchlineRequest(string setup, string punchlineTemplate, string jokeId)
  {
    Setup = setup;
    PunchlineTemplate = punchlineTemplate;
    JokeId = jokeId;
  }
  public string Setup;
  public string PunchlineTemplate;
  public string JokeId;
}

