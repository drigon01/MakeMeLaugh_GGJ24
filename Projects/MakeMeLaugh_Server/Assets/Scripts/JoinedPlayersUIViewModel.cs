using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class JoinedPlayersUIViewModel : MonoBehaviour
{
    private VisualElement rootElement;
    private VisualElement avatarView;
    private UIDocument uiDocument;
    
    private TextElement waitingPlayersTextElement;
    private const string BaseText = "Waiting For Comedians";
    private int numberConnectedClients = 0;
    
    private const float DotUpdateInterval = 0.5f; // Update interval in seconds
    private Coroutine dotCycleCoroutine;
    private Button startGameButton;

    [SerializeField] private VisualTreeAsset UserAvatarTemplate;
    [SerializeField] private int EnableStartAtPlayerCount = 2;
    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        rootElement = uiDocument.rootVisualElement;
    }

    private void Start()
    {
        TransportServer.Instance.OnPlayerMessageReceived += TransportServer_OnPlayerMessageReceived;
        startGameButton = uiDocument.rootVisualElement.Q<Button>("StartGameButton");
        startGameButton.clicked += OnStartGameButtonClicked;
        startGameButton.SetEnabled(false);
        KickOffWaitingText();
    }

    private void addNewPlayerAvatar(string playerName)
    {
        // Check if UIDocument is assigned
        if (uiDocument != null)
        {
            // Load the user avatar template
            var userAvatarElement = UserAvatarTemplate.CloneTree();
            var label =  userAvatarElement.Q<Label>("Label");
            label.text = playerName;
            // Find the existing element with the ID #PlayerRow
            var playerRowElement = uiDocument.rootVisualElement.Q<VisualElement>("PlayerRow");
            
            userAvatarElement.AddToClassList("player_waiting__row");
            // Check if the element with ID #PlayerRow is found
            if (playerRowElement != null)
            {
                // Insert the user avatar element next to the #PlayerRow element
                playerRowElement.Insert(0, userAvatarElement);
            }
            else
            {
                Debug.LogError("Element with ID #PlayerRow not found.");
            }
        }
        else
        {
            Debug.LogError("UIDocument is not assigned.");
        }
    }

    private void KickOffWaitingText()
    {
        // Check if UIDocument is assigned
        if (uiDocument != null)
        {
            // Find the TextElement in the UI
            waitingPlayersTextElement = uiDocument.rootVisualElement.Q<TextElement>("WaitingText");

            // Start the coroutine for dot cycling
            dotCycleCoroutine = StartCoroutine(DotCycleCoroutine());
        }
        else
        {
            Debug.LogError("UIDocument is not assigned.");
        }
    }
    
    IEnumerator DotCycleCoroutine()
    {
        while (true)
        {
            // Update the text with the base text and dots
            waitingPlayersTextElement.text = BaseText + GetDots();

            // Wait for the specified interval before the next update
            yield return new WaitForSeconds(DotUpdateInterval);
        }
    }
    
    string GetDots()
    {
        // Determine the number of dots based on the current time
        int numDots = Mathf.FloorToInt(Time.time % 4);

        // Create a string with the determined number of dots
        return new string('.', numDots);
    }
    
    // Method to stop the coroutine
    public void StopDotCycle()
    {
        if (dotCycleCoroutine != null)
        {
            StopCoroutine(dotCycleCoroutine);
        }
    }

    // Method to remove the text element from the UI
    public void RemoveTextElement()
    {
        if (waitingPlayersTextElement != null)
        {
            waitingPlayersTextElement.RemoveFromHierarchy();
        }
    }
    
    private void TransportServer_OnPlayerMessageReceived(object sender, PlayerMessageEventArgs eventArgs)
    {
        if (eventArgs.EventPlayerMessage.MessageType == MessageType.NEW_CLIENT_CONNECTION)
        {
            StopDotCycle();
            RemoveTextElement();
            numberConnectedClients++;
            addNewPlayerAvatar(eventArgs.EventPlayerMessage.MessageContent);
            if (numberConnectedClients >= EnableStartAtPlayerCount)
            {
                startGameButton.SetEnabled(true);
            }
        }
    }

    private void OnStartGameButtonClicked()
    {
        GameManager.ChangeToWritingRoom();
    }
}
