using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;

public class SessionPlayerListUI : MonoBehaviour
{
    private Transform playerListContainer;
    private GameObject playerNamePrefab;
    private Dictionary<ulong, TextMeshProUGUI> playerNameTexts = new();
    
    [Header("UI Elements to Hide When All Ready")]
    [SerializeField] private GameObject[] elementsToHideWhenReady = new GameObject[3];
    
    private bool hasCheckedReady = false;

    private void Awake()
    {
        // Automatically find the container
        playerListContainer = transform.Find("Player List ScrollView/Viewport/Content");

        if (playerListContainer == null)
        {
            Debug.LogError("Player list container not found in hierarchy!");
            return;
        }

        // Grab the player list item prefab (first child)
        Transform listItem = playerListContainer.Find("Session Player List Item");
        if (listItem != null)
        {
            playerNamePrefab = listItem.gameObject;
            playerNamePrefab.SetActive(false); // hide template
        }
        else
        {
            Debug.LogError("Session Player List Item prefab not found under Content!");
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null) return;

        // Subscribe to player join/leave
        NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;

        // Populate the list with any already-connected clients
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            AddPlayer(client.ClientId);
        }
    }
    
    private void Update()
    {
        // Continuously check ready state if we have 2+ players
        if (!hasCheckedReady && NetworkManager.Singleton != null)
        {
            CheckIfAllPlayersReady();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayer;
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayer;
    }

    private void AddPlayer(ulong clientId)
    {
        Debug.Log($"Adding player {clientId} to session list UI.");
        if (playerNamePrefab == null || playerListContainer == null)
            return;

        // Avoid duplicates
        if (playerNameTexts.ContainsKey(clientId))
            return;

        // Instantiate a new entry
        GameObject entry = Instantiate(playerNamePrefab, playerListContainer);
        entry.SetActive(true);

        // Find TextMeshPro text inside the UI structure
        TextMeshProUGUI text = entry.transform
            .Find("Row/Name Container/Player Name")
            ?.GetComponent<TextMeshProUGUI>();

        if (text == null)
        {
            Debug.LogError("Could not find 'Player Name' text inside list item prefab.");
            return;
        }

        // Try to find the player's NetworkPlayerController so we can display their username
        var playerController = FindPlayerController(clientId);
        if (playerController != null)
        {
            text.text = playerController.GetPlayerName();

            // Subscribe to name changes so UI updates if player changes their name
            playerController.PlayerName.OnValueChanged += (oldValue, newValue) =>
            {
                if (playerNameTexts.TryGetValue(clientId, out var textElement))
                {
                    textElement.text = newValue.ToString();
                }
            };
        }
        else
        {
            text.text = $"Player {clientId}";
        }

        text.color = Color.white;

        playerNameTexts[clientId] = text;
    }

    private NetworkPlayerController FindPlayerController(ulong clientId)
    {
        var players = FindObjectsOfType<NetworkPlayerController>();
        foreach (var p in players)
        {
            if (p.OwnerClientId == clientId) return p;
        }
        return null;
    }

    private void RemovePlayer(ulong clientId)
    {
        if (playerNameTexts.TryGetValue(clientId, out var text))
        {
            Destroy(text.transform.root.gameObject);
            playerNameTexts.Remove(clientId);
        }
    }

    public void UpdatePlayerReadyState(ulong clientId, bool isReady)
    {
        if (playerNameTexts.TryGetValue(clientId, out var text))
        {
            text.color = isReady ? Color.green : Color.white;
        }
    }
    
    private void CheckIfAllPlayersReady()
    {
        var allPlayers = FindObjectsOfType<NetworkPlayerController>();
        
        // Need at least 2 players
        if (allPlayers.Length < 2)
            return;
        
        // Check if all players are ready
        bool allReady = allPlayers.All(player => player.IsReady.Value);
        
        if (allReady)
        {
            HideUIElements();
            NotifyGameManager();
            hasCheckedReady = true; // Only hide once
        }
    }
    
    private void NotifyGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAllPlayersReady();
        }
        else
        {
            Debug.LogWarning("SessionPlayerListUI: GameManager.Instance not found!");
        }
    }
    
    private void HideUIElements()
    {
        Debug.Log("All players ready! Hiding UI elements.");
        
        foreach (var element in elementsToHideWhenReady)
        {
            if (element != null)
            {
                element.SetActive(false);
            }
        }
    }
}
