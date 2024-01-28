public struct PlayerSetupResponse
{
    public PlayerSetupResponse(string setup, string jokeId)
    {
        Setup = setup;
        JokeId = jokeId;
    }
    public string Setup;
    public string JokeId;
}

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
