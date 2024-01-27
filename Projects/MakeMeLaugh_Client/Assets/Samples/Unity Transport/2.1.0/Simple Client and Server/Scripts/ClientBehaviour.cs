using UnityEngine;
using Unity.Networking.Transport;

namespace Unity.Networking.Transport.Samples
{
  public class ClientBehaviour : MonoBehaviour
  {
    NetworkDriver m_Driver;
    NetworkConnection m_Connection;

    [SerializeField] private string _serverIP;
    [SerializeField] private ushort _serverPort = 7777;

    public string ServerIP { get => _serverIP; set => _serverIP = value; }
    public ushort ServerPort { get => _serverPort; set => _serverPort = value; }

    void Start()
    {   
      var endpoint = string.IsNullOrWhiteSpace(ServerIP) ? NetworkEndpoint.LoopbackIpv4.WithPort(ServerPort) :
      NetworkEndpoint.Parse(ServerIP, ServerPort, NetworkFamily.Ipv4);

      Debug.Log($"Connecting to {endpoint.Address} on port {endpoint.Port}");
      m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());

      m_Connection = m_Driver.Connect(endpoint);
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

          uint value = 1;
          m_Driver.BeginSend(m_Connection, out var writer);
          writer.WriteUInt(value);
          m_Driver.EndSend(writer);
        }
        else if (cmd == NetworkEvent.Type.Data)
        {
          uint value = stream.ReadUInt();
          Debug.Log($"Got the value {value} back from the server.");

          m_Connection.Disconnect(m_Driver);
          m_Connection = default;
        }
        else if (cmd == NetworkEvent.Type.Disconnect)
        {
          Debug.Log("Client got disconnected from server.");
          m_Connection = default;
        }
      }
    }
  }
}