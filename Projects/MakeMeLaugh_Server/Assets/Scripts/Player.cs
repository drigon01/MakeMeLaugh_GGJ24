
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Waiting,
    Done,
}

[Serializable]
public class Player
{
    [field:SerializeField] public PlayerState State { get; set; }
    [field:SerializeField] public string Uuid { get; private set; }
    [field:SerializeField] public string Name { get; private set; }
    [field:SerializeField]public int Points { get; set; }

    public Player(string uuid, string name)
    {
        Uuid = uuid;
        Name = name;
        Points = 0;
        State = PlayerState.Done;
        
        // TransportServer.Instance.OnPlayerMessageReceived += TransportServer_OnPlayerMessageReceived;
    }
    
    private void TransportServer_OnPlayerMessageReceived (object sender, PlayerMessageEventArgs eventArgs) {
        Debug.Log("(ATTENTION!) Received the following from the client: " + eventArgs.EventPlayerMessage.MessageType + " " + eventArgs.EventPlayerMessage.MessageContent);

        if(eventArgs.EventPlayerMessage.PlayerUuid != Uuid)
        {
            return;
        }
        
        // Debug.Log("(ATTENTION!) Received the following from the client: " + eventArgs.EventPlayerMessage.MessageType + " " + eventArgs.EventPlayerMessage.MessageContent);
        
        switch (eventArgs.EventPlayerMessage.MessageType)
        {
            case (MessageType.PLAYER_SETUP_RESPONSE):
                Debug.Log("Received a player setup submission");
                break;
            case (MessageType.PLAYER_PUNCHLINE_RESPONSE):
                Debug.Log("Received a player punchline submission");
                break;
            
        }
    }
    
    void UpdatePlayerState()
    {
        // Poll events from players and update state
    }

    public void SendPunchline(Joke joke)
    {
        PlayerPunchlineRequest request = new PlayerPunchlineRequest(joke.Setup, joke.CurrentPunchlineTemplate(), joke.JokeId);
        TransportServer.Instance.SendMessageToPlayer(Uuid,  MessageType.SERVER_PUNCHLINE_REQUEST, JsonUtility.ToJson(request));
        State = PlayerState.Waiting;
    }

    public void SendSetupTemplate(Joke joke)
    {
        // Push setup to client 
        // "Why did the _BLANK_ cross the road?";
        // "Why did the <filled> cross the road?";
        PlayerSetupRequest request = new PlayerSetupRequest(joke.Setup, joke.JokeId);
        TransportServer.Instance.SendMessageToPlayer(Uuid,  MessageType.SERVER_SETUP_REQUEST, JsonUtility.ToJson(request));
        State = PlayerState.Waiting;
    }
    
}
