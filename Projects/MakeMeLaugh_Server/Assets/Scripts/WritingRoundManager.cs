using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

enum JokeState
{
    setup,
    Punchline,
    Done,
}    

public class WritingRoundManager: MonoBehaviour
{
    private JokeState m_state = JokeState.setup;
    
    private List<Player> m_players;
    private List<Joke> m_jokes;
    private int m_currentPlayerIndex;
    
    public TextMeshPro m_text;
    
    public void Start()
    {
        m_text.text = "Hello World";
        m_players = GetComponentInParent<GameManager>().GetPlayers();
        m_jokes = new List<Joke>();
        
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
    }
    
    void Update()
    {
        if (!CheckPlayersDone())
        {
            m_text.text = "Waiting for Players";
            return;
        }
        if (m_state == JokeState.setup)
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
            return;
            
        }
        throw new System.Exception("RoundManager in invalid state");
    }

    void RunSetup()
    {
        Debug.Log("Running Setup with " + m_players.Count + " players and " + m_jokes.Count + " jokes");
        for(int i = 0; i < m_players.Count; i++)
        {
            m_players[i].SendSetupTemplate(m_jokes[i].GetSetup());
        }  
        m_state = JokeState.Punchline;
    }
    
    bool CheckPlayersDone()
    {
        foreach (Player player in m_players)
        {
            if (player.GetState() != PlayerState.Done)
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
            if (!joke.IsDone())
            {
                return false;
            }
        }
        return true;
    }
}
