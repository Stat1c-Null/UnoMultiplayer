using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Player Name Card UI")]
    [SerializeField] private GameObject playerNameCardPrefab;
    [SerializeField] private Transform playerNameCardContainer;
    
    private Dictionary<ulong, GameObject> playerNameCards = new();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    /// <summary>
    /// Called by SessionPlayerListUI when all players are ready.
    /// Creates UI cards for each player showing their name and card count.
    /// </summary>
    public void OnAllPlayersReady()
    {
        Debug.Log("GameManager: All players ready! Creating player name cards.");
        CreatePlayerNameCards();
    }
    
    private void CreatePlayerNameCards()
    {
        if (playerNameCardPrefab == null)
        {
            Debug.LogError("GameManager: PlayerNameCard prefab not assigned!");
            return;
        }
        
        if (playerNameCardContainer == null)
        {
            Debug.LogError("GameManager: PlayerNameCard container not assigned!");
            return;
        }
        
        // Clear any existing cards
        ClearPlayerNameCards();
        
        // Get all connected players
        var allPlayers = FindObjectsOfType<NetworkPlayerController>();
        
        foreach (var player in allPlayers)
        {
            CreatePlayerNameCard(player);
        }
    }
    
    private void CreatePlayerNameCard(NetworkPlayerController player)
    {
        ulong clientId = player.OwnerClientId;
        
        // Avoid duplicates
        if (playerNameCards.ContainsKey(clientId))
            return;
        
        // Instantiate the prefab
        GameObject card = Instantiate(playerNameCardPrefab, playerNameCardContainer);
        card.SetActive(true);
        
        // Navigate to the text components
        // Structure: PlayerNameCard -> PlayerNamecardBackboard -> (PlayerName, CardsRemaining)
        Transform backboard = card.transform.Find("PlayerNameBackboard");
        if (backboard == null)
        {
            Debug.LogError("GameManager: Could not find PlayerNameBackboard in prefab!");
            Destroy(card);
            return;
        }
        
        TextMeshProUGUI playerNameText = backboard.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cardsRemainingText = backboard.Find("CardsRemaining")?.GetComponent<TextMeshProUGUI>();
        
        if (playerNameText == null || cardsRemainingText == null)
        {
            Debug.LogError("GameManager: Could not find PlayerName or CardsRemaining text in prefab!");
            Destroy(card);
            return;
        }
        
        // Set the player name
        playerNameText.text = player.GetPlayerName();
        
        // Set cards remaining (0 for now, will be updated when card logic is implemented)
        cardsRemainingText.text = "Cards: 0";
        
        // Subscribe to name changes
        player.PlayerName.OnValueChanged += (oldValue, newValue) =>
        {
            if (playerNameText != null)
            {
                playerNameText.text = newValue.ToString();
            }
        };
        
        playerNameCards[clientId] = card;
        
        Debug.Log($"GameManager: Created name card for player {player.GetPlayerName()} (ID: {clientId})");
    }
    
    /// <summary>
    /// Updates the card count display for a specific player.
    /// Call this when a player's hand changes.
    /// </summary>
    public void UpdatePlayerCardCount(ulong clientId, int cardCount)
    {
        if (playerNameCards.TryGetValue(clientId, out GameObject card))
        {
            Transform backboard = card.transform.Find("PlayerNameCardBackboard");
            TextMeshProUGUI cardsRemainingText = backboard?.Find("CardsRemaining")?.GetComponent<TextMeshProUGUI>();
            
            if (cardsRemainingText != null)
            {
                cardsRemainingText.text = $"Cards: {cardCount}";
            }
        }
    }
    
    private void ClearPlayerNameCards()
    {
        foreach (var card in playerNameCards.Values)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        playerNameCards.Clear();
    }
    
    /// <summary>
    /// Removes a specific player's name card (e.g., when they disconnect).
    /// </summary>
    public void RemovePlayerNameCard(ulong clientId)
    {
        if (playerNameCards.TryGetValue(clientId, out GameObject card))
        {
            if (card != null)
            {
                Destroy(card);
            }
            playerNameCards.Remove(clientId);
        }
    }
}
