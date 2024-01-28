using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UIElements;

public class RelayStateManager : MonoBehaviour
{
    private Task _initAndAuthTask;

    public string JoinCode;
    
    void Start()
    {
        _initAndAuthTask = InitializeAndAuthenticate();
    }

    private async Task InitializeAndAuthenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartHostingGame(int maxConnections)
    {
        await _initAndAuthTask;
        
        var hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        var relayServerData = new RelayServerData(hostAllocation, "udp");
        TransportServer.Instance.StartServerWithRelay(relayServerData);

        JoinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

        GetComponent<UIDocument>().rootVisualElement.Q<Label>("JOIN_CODE").text = JoinCode;
    }
}
