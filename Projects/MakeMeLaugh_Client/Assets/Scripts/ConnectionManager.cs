using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;


public class ConnectionManager
{
  [SerializeField] private string serverAddress = "0.0.0.0";
  [SerializeField] private ushort serverPort = 7771;

  private NetworkDriver _driver;
  private NetworkConnection _connection;

  private string ClientUuid;

  public ConnectionManager(string ip, ushort port)
  {
    serverAddress = ip;
    serverPort = port;

    ClientUuid = System.Guid.NewGuid().ToString();

    _driver = NetworkDriver.Create(new WebSocketNetworkInterface());
    var endpoint = NetworkEndpoint.Parse(serverAddress, serverPort);

    Debug.Log($"Initializing client {ClientUuid}");
    Debug.Log($"Connecting to {endpoint}");
    _connection = _driver.Connect(endpoint);
    _driver.ScheduleUpdate().Complete();

    if (_connection.IsCreated)
    {
      Debug.Log("Created connection: " + _connection);
    }
    else
    {
      Debug.Log("Connection missing " + _connection);
    }
  }

  public void ExecuteUpdate()
  {
    _driver.ScheduleUpdate().Complete();

    if (!_connection.IsCreated)
    {
      return;
    }

    DataStreamReader stream;
    NetworkEvent.Type cmd;
    while ((cmd = _connection.PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
    {
      if (cmd == NetworkEvent.Type.Connect)
      {
        Debug.Log("We are now connected to the server.");

        // Send the handshake message including the client ID (uuid)
        PlayerMessage handshakeMessage = new PlayerMessage(ClientUuid, MessageType.NEW_CLIENT_CONNECTION, "test submission");
        _driver.BeginSend(_connection, out var writer);
        NativeArray<byte> messageBytes = PlayerMessage.GetBytes(handshakeMessage);

        writer.WriteBytes(messageBytes);

        _driver.EndSend(writer);
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
        _connection = default;
      }
    }
  }
}

