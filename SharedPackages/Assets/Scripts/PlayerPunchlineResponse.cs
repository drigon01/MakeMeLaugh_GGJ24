public struct PlayerPunchlineResponse
{
  public PlayerPunchlineResponse(string punchlineSegment, string jokeId)
  {
    PunchlineSegment = punchlineSegment;
    JokeId = jokeId;
  }
  public string PunchlineSegment;
  public string JokeId;
}

