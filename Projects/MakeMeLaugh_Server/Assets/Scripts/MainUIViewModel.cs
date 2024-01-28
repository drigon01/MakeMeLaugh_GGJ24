using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;

public static class IPManager
{
    public static string GetLocalIPAddress()
    {
        var nics = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var nic in nics)
        {
            if(nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
            }  
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}

public class MainUIViewModel : MonoBehaviour
{
    private VisualElement _rootElement;
    private VisualElement _settingsView;

    [SerializeField] private VisualTreeAsset _serverSettingsTemplate;
    [SerializeField] private ushort _port;
    [SerializeField] private string _ip;
    [SerializeField] private TransportServer _transportServer;
    [SerializeField] private GameObject waitingPlayersUI;
    
    // Start is called before the first frame update

    private void Awake()
    {
        var document = GetComponent<UIDocument>();
        _rootElement = document.rootVisualElement;
    }

    void Start()
    {
        _settingsView = new VisualElement();
        _serverSettingsTemplate.CloneTree(_settingsView);

        var serveButton = _settingsView.Q<Button>("SERVE");
        var serverIP = _settingsView.Q<Label>("IP_VALUE");
        var serverPort = _settingsView.Q<TextField>("PORT");

        serveButton.clicked += OnServeButtonClicked;
        
        var ip = IPManager.GetLocalIPAddress();

        _ip = ip;
        _port = ushort.Parse("7777");

        Debug.Log(ip);
        serverIP.text = ip;
        serverPort.value = _port.ToString();
        
        serverPort.RegisterValueChangedCallback(OnPortChanged);

        ShowPopUp(_settingsView);
    }

    private void OnPortChanged(ChangeEvent<string> evt)
    {
        if (ushort.TryParse(evt.newValue, out var result))
        {
            _port = result;
        }
        else
        {
            throw new ArgumentException("Incorrect value provided as port");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnServeButtonClicked()
    {
        _transportServer.StartServer(_ip, _port);
        // GameManager.ChangeToWritingRoom();
        waitingPlayersUI.SetActive(true);
        //TODO: add actual connection logic
        ClosePopUp(_settingsView);
    }
    
    private void ShowPopUp(VisualElement popup)
    {
        popup.AddToClassList("popup");
        _rootElement.Insert(0, popup);
    }

    private void ClosePopUp(VisualElement popup)
    {
        _rootElement.Remove(popup);
    }
}