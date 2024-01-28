using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class WritingRoundManager: MonoBehaviour
{
    private JokeState m_state = JokeState.Setup;
    
    private List<Player> m_players;
    private List<Joke> m_jokes;
    private int m_currentPlayerIndex;
    
    public TextMeshPro m_text;
    
    public void Start()
    {
        m_text.text = "Hello World";
        m_players = GameObject.Find("GameManager").GetComponent<GameManager>().GetPlayers();
        m_jokes = new List<Joke>();

        foreach (var player in m_players)
        {
            Debug.Log("Player: " + player.Name);
        }
        
        for (int i = 0; i < m_players.Count; i++)
        {  
            Player author = m_players[i];
            List<Player> playersNoAuthor = new List<Player>(m_players);
            playersNoAuthor.Remove(author);
            playersNoAuthor.AddRange(playersNoAuthor);
            
            // NOTTODO: Do not try to understand this code
            List<Player> coauthors = new List<Player>();
            for (int j = i; j < i + Joke.MAX_SEGMENTS; j++)
            {
                coauthors.Add(playersNoAuthor[j]);
            }
            
            m_jokes.Add(new Joke(author, coauthors));
        }
    
    TransportServer.Instance.OnPlayerMessageReceived += TransportServer_OnPlayerMessageReceived;
}

    void OnDestroy()
    {
        TransportServer.Instance.OnPlayerMessageReceived -= TransportServer_OnPlayerMessageReceived;
    }
    
    private void TransportServer_OnPlayerMessageReceived (object sender, PlayerMessageEventArgs eventArgs) {
        Debug.Log("(ATTENTION!) Received the following from the client: " + eventArgs.EventPlayerMessage.MessageType + " " + eventArgs.EventPlayerMessage.MessageContent);

        Player relevantPlayer = m_players.Find(player => player.Uuid == eventArgs.EventPlayerMessage.PlayerUuid);

        if(relevantPlayer == null)
        {
            Debug.LogError("Received message from unknown player");
            return;
        }
        // Debug.Log("(ATTENTION!) Received the following from the client: " + eventArgs.EventPlayerMessage.MessageType + " " + eventArgs.EventPlayerMessage.MessageContent);
            
        switch (eventArgs.EventPlayerMessage.MessageType)
        {
            case (MessageType.PLAYER_SETUP_RESPONSE):
                Debug.Log("Received a player setup submission");
                PlayerSetupResponse response = JsonUtility.FromJson<PlayerSetupResponse>(eventArgs.EventPlayerMessage.MessageContent);
                Joke setupJoke = m_jokes.Find(joke => joke.JokeId == response.JokeId);
                setupJoke.Setup = response.Setup;
                relevantPlayer.State = PlayerState.Done;
                break;
            case (MessageType.PLAYER_PUNCHLINE_RESPONSE):
                Debug.Log("Received a player punchline submission");
                PlayerPunchlineResponse punchlineResponse = JsonUtility.FromJson<PlayerPunchlineResponse>(eventArgs.EventPlayerMessage.MessageContent);
                Joke relevantJoke = m_jokes.Find(joke => joke.JokeId == punchlineResponse.JokeId);
                relevantJoke.AddPunchlineSegmentText(punchlineResponse.PunchlineSegment);
                relevantPlayer.State = PlayerState.Done;
                break;
        }
    }
    void Update()
    {
        if (!CheckPlayersDone())
        {
            m_text.text = "Waiting for Players";
            return;
        }
        if (m_state == JokeState.Setup)
        {
            Debug.Log("Running Setup");
            m_text.text = "Running Setup";
            RunSetup();
            return;
        }
        if(m_state == JokeState.Punchline)
        {
            Debug.Log("Running Punchline");
            m_text.text = "Running Punchline";
            RunPunchline();
            return;
        }

        if (m_state == JokeState.Done)
        { 
            // Debug.Log("RoundManager Done");
            m_text.text = "Awesome Jokes Everyone!";
            // SwitchScene();
            // print out jokes
            foreach (Joke joke in m_jokes)
            {
                Debug.Log(joke.CompletedJoke);
            }
            return;
            
        }
        throw new System.Exception("RoundManager in invalid state");
    }

    void RunSetup()
    {
        Debug.Log("Running Setup with " + m_players.Count + " players and " + m_jokes.Count + " jokes");
        for(int i = 0; i < m_players.Count; i++)
        {
            m_players[i].SendSetupTemplate(m_jokes[i]);
        }  
        m_state = JokeState.Punchline;
    }
    
    bool CheckPlayersDone()
    {
        foreach (Player player in m_players)
        {
            if (player.State != PlayerState.Done)
            {
                return false;
            }
        }
        return true;
    }
    
    private void RunPunchline()
    {   
        if(AreJokesDone())
        {
            m_state = JokeState.Done;
            return;
        }
        
       foreach(Joke joke in m_jokes)
       {
           joke.RunNextPunchlineSegment();
       }
    }

    private bool AreJokesDone()
    {
        foreach (Joke joke in m_jokes)
        {
            if (!joke.IsPunchlineComplete())
            {
                return false;
            }
        }
        return true;
    }
}
