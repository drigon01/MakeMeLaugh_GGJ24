using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private List<Player> m_players = new List<Player>{};
    
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        TransportServer.Instance.OnPlayerMessageReceived += TransportServer_OnPlayerMessageReceived;
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
    
    

    private void TransportServer_OnPlayerMessageReceived(object sender, PlayerMessageEventArgs eventArgs)
    {
        Debug.Log("(ATTENTION!) Received the following from the client: " + eventArgs.EventPlayerMessage.MessageType +
                  " " + eventArgs.EventPlayerMessage.MessageContent);
        
        switch (eventArgs.EventPlayerMessage.MessageType)
        {
            case (MessageType.NEW_CLIENT_CONNECTION):
                Debug.Log("GameManager Received a new client connection");
                string [] names = {"Jami", "Olga", "Kalman", "Layla", "James", "Richard", "August", "Lily", "Bob", "PotatoMan"};
                Player player = new Player(eventArgs.EventPlayerMessage.PlayerUuid, names[Random.Range(0, names.Length)]);
                m_players.Add(player);
                break;
        }
    }
    public static void ChangeToWritingRoom()
    {
        TransportServer.Instance.BroadcastMessage(MessageType.SERVER_SCENE_CHANGE_WRITING_ROOM, "");
        SceneManager.LoadScene("WritingRoom");
    }
}
