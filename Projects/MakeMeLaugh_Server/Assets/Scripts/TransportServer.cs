using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class TransportServer : MonoBehaviour
{
    
    NetworkDriver m_Driver;
    NativeList<NetworkConnection> m_Connections;
    private Dictionary<string, NetworkConnection> playerToNetworkConnection = new Dictionary<string, NetworkConnection>();

    [SerializeField] private ushort serverPort = 7771;
    [SerializeField] private int serverPlayerCapacity = 4;

    
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connections = new NativeList<NetworkConnection>(serverPlayerCapacity, Allocator.Persistent);

        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(serverPort);
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port 7777.");
            return;
        }
        m_Driver.Listen();
        Debug.Log($"Started listening on {endpoint.Address}:{endpoint.Port}");
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

                    NativeArray<byte> rawMessage = new NativeArray<byte>(stream.Length, Allocator.Persistent);
                    stream.ReadBytes(rawMessage);
                    PlayerMessage playerMessage = PlayerMessage.FromBytes(rawMessage);
                    if (playerMessage.MessageType == ClientToServerMessageType.NEW_CLIENT_CONNECTION)
                    {
                        registerNewPlayer(playerMessage, m_Connections[i]);
                    }

                    rawMessage.Dispose();
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
        Debug.Log("Registering new client (player): " + playerMessage.PlayerUuid);
        playerToNetworkConnection.Add(playerMessage.PlayerUuid, networkConnection);
    }
}
