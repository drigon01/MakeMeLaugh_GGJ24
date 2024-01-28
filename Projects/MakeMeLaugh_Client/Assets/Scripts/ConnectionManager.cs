using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;


public class ConnectionManager
{
  private NetworkDriver _driver;
  private NetworkConnection _connection;

  private string _clientUuid;
  private string _name;

  public string ClientUUID => _clientUuid;

  public event Action Connected;
  public event Action<PlayerSetupRequest> JokeSetupRequested;
  public event Action<PlayerPunchlineRequest> JokePunchlineRequested;

  public ConnectionManager(string address, ushort port, string name)
  {
    _clientUuid = System.Guid.NewGuid().ToString();
    _name = name;

    _driver = NetworkDriver.Create(new WebSocketNetworkInterface());
    var endpoint = NetworkEndpoint.Parse(address, port);

    Debug.Log($"Initializing client {_clientUuid}");
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
        PlayerMessage handshakeMessage = new PlayerMessage(_clientUuid, MessageType.NEW_CLIENT_CONNECTION, _name);
        SendMessageToServer(handshakeMessage);
        Debug.Log("Done with the message sending from the client");
      }
      else if (cmd == NetworkEvent.Type.Data)
      {
        FixedString4096Bytes rawMessage = new FixedString4096Bytes();
        rawMessage = stream.ReadFixedString4096();
        PlayerMessage playerMessage = JsonUtility.FromJson<PlayerMessage>(rawMessage.ToString());
        Debug.Log("Got a message from server " + playerMessage.MessageContent);

        // _connection.Disconnect(m_Driver);
        // _connection = default;
        if (playerMessage.MessageType == MessageType.SERVER_SETUP_REQUEST)
        {
          Debug.Log("Client got a setup request from server");
          PlayerSetupRequest request = JsonUtility.FromJson<PlayerSetupRequest>(playerMessage.MessageContent);
          JokeSetupRequested?.Invoke(request);
        }
        else if (playerMessage.MessageType == MessageType.SERVER_PUNCHLINE_REQUEST)
        {
          Debug.Log("Client got a punchline request from server");
          PlayerPunchlineRequest request = JsonUtility.FromJson<PlayerPunchlineRequest>(playerMessage.MessageContent);
          JokePunchlineRequested?.Invoke(request);
          
        }
      }
      else if (cmd == NetworkEvent.Type.Disconnect)
      {
        Debug.Log("Client got disconnected from server.");
        _connection = default;
      }
    }
  }

  public void SendMessageToServer<T>(T message)
  {
    _driver.BeginSend(_connection, out var writer);
    string json = JsonUtility.ToJson(message);

    writer.WriteFixedString4096(json);

    _driver.EndSend(writer);
  }
}

