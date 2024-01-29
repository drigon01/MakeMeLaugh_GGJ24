using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
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
  public event Action SceneTransitionRequested;

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

  private Task _relayConnectTask;

  public ConnectionManager(string joinCode, string name)
  {
    _clientUuid = System.Guid.NewGuid().ToString();
    _name = name;

    _relayConnectTask = ConnectWithJoinCode(joinCode);
  }

  private async Task ConnectWithJoinCode(string joinCode)
  {
    await UnityServices.InitializeAsync();
    await AuthenticationService.Instance.SignInAnonymouslyAsync();
    var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

    string connectionType;
#if PLATFORM_WEBGL
    connectionType = "wss";
#else
    connectionType = "udp";
#endif

    var relayData = new RelayServerData(allocation, connectionType);

    var settings = new NetworkSettings();
    settings.WithRelayParameters(ref relayData);

#if PLATFORM_WEBGL
      _driver = NetworkDriver.Create(new WebSocketNetworkInterface(), settings);
#else
    _driver = NetworkDriver.Create(settings);
#endif

    _driver.Bind(NetworkEndpoint.AnyIpv4);
    _connection = _driver.Connect();

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
    if (_relayConnectTask != null && _relayConnectTask.IsCompleted)
    {
      if (_relayConnectTask.IsFaulted)
        Debug.LogException(_relayConnectTask.Exception);
      _relayConnectTask = null;
    }

    if (!_driver.IsCreated)
      return;

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

        switch (playerMessage.MessageType)
        {
          case MessageType.SERVER_SETUP_REQUEST:
            {
              Debug.Log("Client got a setup request from server");
              PlayerSetupRequest request = JsonUtility.FromJson<PlayerSetupRequest>(playerMessage.MessageContent);
              JokeSetupRequested?.Invoke(request);
              break;
            }
          case MessageType.SERVER_PUNCHLINE_REQUEST:
            {
              Debug.Log("Client got a punchline request from server");
              PlayerPunchlineRequest request = JsonUtility.FromJson<PlayerPunchlineRequest>(playerMessage.MessageContent);
              JokePunchlineRequested?.Invoke(request);
              break;
            }
          case MessageType.SERVER_SCENE_CHANGE_STAGE:
            {
              SceneTransitionRequested?.Invoke(); break;
            }
        }
      }
      else if (cmd == NetworkEvent.Type.Disconnect)
      {
        Debug.Log("Client got disconnected from server.");
        _connection = default;
      }
    }
  }

  public void SendMessageToServer(PlayerMessage message)
  {
    _driver.BeginSend(_connection, out var writer);
    string json = JsonUtility.ToJson(message);

    writer.WriteFixedString4096(json);

    _driver.EndSend(writer);
  }
}

