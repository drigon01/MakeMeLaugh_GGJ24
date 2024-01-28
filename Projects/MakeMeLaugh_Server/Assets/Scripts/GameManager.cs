using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private List<Player> m_players = new List<Player>
    {
        new Player("hi", "Jami"), new Player("hi1", "Olga"), new Player("hi2", "Kalman"), new Player("hi3", "Layla")
    };

    // Start is called before the first frame update
    void Start()
    {       
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public List<Player> GetPlayers()
    {
        Debug.Log("Returning players");
        return m_players;
    }
}
