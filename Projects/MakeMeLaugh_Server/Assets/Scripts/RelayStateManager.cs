using System;
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
    private VisualElement _overlay;

    public string JoinCode;
    public string ClientHostURI = "https://richard-fine.itch.io/the-laugh-hole?secret=jal3L5588KAmK3SErcfq9IpPAw";
    
    void Start()
    {
        _overlay = GetComponent<UIDocument>().rootVisualElement;
        _overlay.visible = false;
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
        _overlay.Q<Label>("JOIN_CODE").text = JoinCode;

        var qrCode = _overlay.Q<QRCoder.Unity.QRCodeDisplay>("QR_CODE");
        if (!string.IsNullOrWhiteSpace(ClientHostURI))
        {
            var uri = new UriBuilder(ClientHostURI);
            uri.Query = string.Join("&", uri.Query, "joinCode=" + JoinCode);
            qrCode.value = uri.ToString();
            qrCode.visible = true;
        }
        else
        {
            qrCode.visible = false;
        }

        _overlay.visible = true;
    }
}
