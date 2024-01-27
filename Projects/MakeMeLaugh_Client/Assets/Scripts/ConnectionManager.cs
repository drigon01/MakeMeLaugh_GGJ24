using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;


public class ConnectionManaager : MonoBehaviour
{
  NetworkDriver m_Driver;
  NetworkConnection m_Connection;
  [SerializeField] private string serverAddress = "0.0.0.0";
  [SerializeField] private ushort serverPort = 7771;

  private string ClientUuid;
  void Start()
  {
    m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
    ClientUuid = System.Guid.NewGuid().ToString();
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
        NativeArray<byte> messageBytes = PlayerMessage.GetBytes(handshakeMessage);

        writer.WriteBytes(messageBytes);

        m_Driver.EndSend(writer);
        messageBytes.Dispose();
        Debug.Log("Done with the message sending");

      }
      else if (cmd == NetworkEvent.Type.Data)
      {

        NativeArray<byte> rawMessage = new NativeArray<byte>(stream.Length, Allocator.Persistent);
        stream.ReadBytes(rawMessage);
        PlayerMessage playerMessage = PlayerMessage.FromBytes(rawMessage);
        Debug.Log("Got a message from server " + playerMessage.MessageContent);

        // m_Connection.Disconnect(m_Driver);
        // m_Connection = default;
      }
      else if (cmd == NetworkEvent.Type.Disconnect)
      {
        Debug.Log("Client got disconnected from server.");
        m_Connection = default;
      }
    }
  }
}

