using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class TransportServer : MonoBehaviour
{
    public static TransportServer Instance { get; private set; }

    NetworkDriver m_Driver;
    NativeList<NetworkConnection> m_Connections;
    public event EventHandler<PlayerMessageEventArgs> OnPlayerMessageReceived;

    private Dictionary<string, NetworkConnection> playerToNetworkConnection = new Dictionary<string, NetworkConnection>();

    [SerializeField] private ushort serverPort = 7771;
    [SerializeField] private int serverPlayerCapacity = 4;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one instance of TransportServer found! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
    public void StartServerer()
    {
        m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
        m_Connections = new NativeList<NetworkConnection>(serverPlayerCapacity, Allocator.Persistent);

        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(serverPort);
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port " + serverPort);
            return;
        }
        m_Driver.Listen();
        Debug.Log($"Started listening on {endpoint.Address}");
    }
    
    void OnDestroy()
    {
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections.
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                i--;
            }
        }

        // Accept new connections.
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default)
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection.");
        }
        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    Debug.Log("Accepting data from the client");
                    FixedString4096Bytes rawMessage = new FixedString4096Bytes();
                    rawMessage = stream.ReadFixedString4096();
                    PlayerMessage playerMessage = JsonUtility.FromJson<PlayerMessage>(rawMessage.ToString());
                    Debug.Log("Received the following from the client: " + playerMessage.MessageType + " " + playerMessage.MessageContent);

                    if (playerMessage.MessageType == MessageType.NEW_CLIENT_CONNECTION)
                    {
                        registerNewPlayer(playerMessage, m_Connections[i]);

                        
                        // EXAMPLE: how to send data to a specific player
                        // SendMessageToPlayer(playerMessage.PlayerUuid, MessageType.PLAYER_ANSWER_SUBMISSION, "hello from server");
                    }
                    OnPlayerMessageReceived?.Invoke(this, new PlayerMessageEventArgs(playerMessage));

                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    m_Connections[i] = default;
                    break;
                }
            }
        }
    }

    private void registerNewPlayer(PlayerMessage playerMessage, NetworkConnection networkConnection)
    {
        Debug.Log("Registering new client (player): " + playerMessage.MessageContent);
        playerToNetworkConnection.Add(playerMessage.PlayerUuid, networkConnection);
    }

    public void BroadcastMessage(MessageType messageType, string messageContent)
    {
        foreach (var playerPair in playerToNetworkConnection)
        {
            SendMessageToPlayer(playerPair.Key, messageType, messageContent);
        }
    }

    public void SendMessageToPlayer(string playerUuid, MessageType messageType, string messageContent)
    {
        PlayerMessage message = new PlayerMessage(playerUuid, messageType, messageContent);
        string jsonMessage = JsonUtility.ToJson(message); 
        m_Driver.BeginSend(NetworkPipeline.Null, playerToNetworkConnection[playerUuid], out var writer);
        writer.WriteFixedString4096(jsonMessage);
        m_Driver.EndSend(writer);
    }
}
