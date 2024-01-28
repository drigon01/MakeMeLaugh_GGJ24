public struct PlayerSetupRequest
{
  public PlayerSetupRequest(string setupTemplate, string jokeId)
  {
    SetupTemplate = setupTemplate;
    JokeId = jokeId;
  }
  public string SetupTemplate;
  public string JokeId;
}

