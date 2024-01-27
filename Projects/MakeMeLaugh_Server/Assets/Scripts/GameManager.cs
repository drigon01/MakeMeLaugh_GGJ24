using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private List<Player> m_players = new List<Player>
    {
        new Player(1, "Jami"), new Player(2, "Olga"), new Player(3, "Kalman"), new Player(4, "Layla"),
        new Player(5, "Richard"), new Player(6, "James"), new Player(7, "August")
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
        return m_players;
    }
}
