
using System.Collections.Generic;

public enum PlayerState
{
    Waiting,
    Done,
}

public class Player
{
    private PlayerState m_state;
    private int m_id;
    private string m_name;
    
    public Player(int id, string name)
    {
        m_id = id;
        m_name = name;
        m_state = PlayerState.Done;
    }
    
    
    void UpdatePlayerState()
    {
        // Poll events from players and update state
    }

    public void SendPunchline(string setup, List<PunchlineSegment> punchlineSegments)
    {
        m_state = PlayerState.Waiting;
        m_state = PlayerState.Done;
    }

    public void SendSetupTemplate(string template)
    {
        // Push setup to client 
        // "Why did the _BLANK_ cross the road?";
        // "Why did the <filled> cross the road?";

        m_state = PlayerState.Waiting;
        m_state = PlayerState.Done;
    }
    
    public PlayerState GetState()
    {
        return m_state;
    }
    
    public string GetName()
    {
         return m_name;
    }
    
    public int GetId()
    {
        return m_id;
    }
}
