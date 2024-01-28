using System;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;
using static System.Guid;

public class ClientTester : MonoBehaviour {
    NetworkDriver m_Driver;
    NetworkConnection m_Connection;
    [SerializeField] private string serverAddress = "0.0.0.0";
    [SerializeField] private ushort serverPort = 7771;

    public string ClientUuid;
    
    void Start()
    {
        m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
        // ClientUuid = Guid.NewGuid().ToString();
        // ClientUuid = "hi";

        var endpoint = NetworkEndpoint.Parse(serverAddress, serverPort);
        
        Debug.Log($"Initializing client {ClientUuid}");
        Debug.Log($"Connecting to {endpoint}");
        m_Connection = m_Driver.Connect(endpoint);
        m_Driver.ScheduleUpdate().Complete();

        if (m_Connection.IsCreated)
        {
            Debug.Log("Created connection: " + m_Connection);
        }
        else
        {
            Debug.Log("Connection missing " + m_Connection);
        }
    }

    void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.S)) {
            Debug.Log("CLIENT SETUP SEND");

            PlayerMessage message = new PlayerMessage(ClientUuid, MessageType.PLAYER_SETUP_RESPONSE, JsonUtility.ToJson(new PlayerSetupResponse("HERE IS MY SETUP", "1")));
            m_Driver.BeginSend(m_Connection, out var writer);
            NativeArray<byte> messageBytes = PlayerMessage.GetBytes(message);
                
            writer.WriteBytes(messageBytes);
                
            m_Driver.EndSend(writer);
            messageBytes.Dispose();
        }
        
        if (Input.GetKeyDown(KeyCode.P)) {
            Debug.Log("CLIENT PUNCHLINE SEND");

            PlayerMessage message = new PlayerMessage(ClientUuid, MessageType.PLAYER_PUNCHLINE_RESPONSE, JsonUtility.ToJson(new PlayerPunchlineResponse("HERE IS MY PUNCHLINE", "1")));
            ;
            m_Driver.BeginSend(m_Connection, out var writer);
            NativeArray<byte> messageBytes = PlayerMessage.GetBytes(message);
                
            writer.WriteBytes(messageBytes);
                
            m_Driver.EndSend(writer);
            messageBytes.Dispose();
        }

        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server.");

                // Send the handshake message including the client ID (uuid)
                PlayerMessage handshakeMessage = new PlayerMessage(ClientUuid, MessageType.NEW_CLIENT_CONNECTION, "test submission");
                m_Driver.BeginSend(m_Connection, out var writer);
                string json = JsonUtility.ToJson(handshakeMessage);
                
                writer.WriteFixedString4096(json);
                
                m_Driver.EndSend(writer);
                Debug.Log("Done with the message sending from the client");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                FixedString4096Bytes rawMessage = new FixedString4096Bytes();
                rawMessage = stream.ReadFixedString4096();
                PlayerMessage playerMessage = JsonUtility.FromJson<PlayerMessage>(rawMessage.ToString());
                Debug.Log("Got a message from server " + playerMessage.MessageContent);

                // m_Connection.Disconnect(m_Driver);
                // m_Connection = default;
                
                // WRITING ROUND EVENTS
                if (playerMessage.MessageType == MessageType.SERVER_SETUP_REQUEST)
                {
                    Debug.Log("Client got a setup request from server");
                    PlayerPunchlineRequest request = JsonUtility.FromJson<PlayerPunchlineRequest>(playerMessage.MessageContent);

                    PlayerMessage message = new PlayerMessage(ClientUuid, MessageType.PLAYER_SETUP_RESPONSE, JsonUtility.ToJson(new PlayerSetupResponse("HERE IS MY SETUP", request.JokeId)));
                    m_Driver.BeginSend(m_Connection, out var writer);
                    NativeArray<byte> messageBytes = PlayerMessage.GetBytes(message);
                
                    writer.WriteBytes(messageBytes);
                
                    m_Driver.EndSend(writer);
                    messageBytes.Dispose();
                }
                else if (playerMessage.MessageType == MessageType.SERVER_PUNCHLINE_REQUEST)
                {
                    Debug.Log("Client got a punchline request from server");
                    PlayerSetupRequest request = JsonUtility.FromJson<PlayerSetupRequest>(playerMessage.MessageContent);

                    PlayerMessage message = new PlayerMessage(ClientUuid, MessageType.PLAYER_PUNCHLINE_RESPONSE, JsonUtility.ToJson(new PlayerPunchlineResponse("HERE IS MY PUNCHLINE", request.JokeId)));
                    m_Driver.BeginSend(m_Connection, out var writer);
                    NativeArray<byte> messageBytes = PlayerMessage.GetBytes(message);
                
                    writer.WriteBytes(messageBytes);
                
                    m_Driver.EndSend(writer);
                    messageBytes.Dispose();
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server.");
                m_Connection = default;
            }
        }
    }
}
