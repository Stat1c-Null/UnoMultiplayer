using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server-authoritative card manager for an Uno game.
/// Builds the 108-card deck, shuffles, deals 7 cards per player,
/// and flips the first discard card. Each player receives only their
/// own hand via a targeted ClientRpc.
///
/// Attach this to the same GameObject as GameManager (must have a NetworkObject component).
/// </summary>
public class CardManager : NetworkBehaviour
{
    // ─── Events ───────────────────────────────────────────────────────
    /// <summary>Fired on the local client when its hand is updated.</summary>
    public event Action<List<CardData>> OnLocalHandUpdated;

    /// <summary>Fired on every client when the top discard card changes.</summary>
    public event Action<CardData> OnTopDiscardChanged;

    // ─── Card Visuals (assign in Inspector) ───────────────────────────
    [Header("Card Visuals")]
    [Tooltip("ScriptableObject that maps every CardData to its sprite.")]
    [SerializeField] private CardSpriteDatabase cardSpriteDatabase;

    [Tooltip("Prefab with an Image + CardUI component. One is instantiated per card in the player's hand.")]
    [SerializeField] private GameObject cardPrefab;

    [Tooltip("Parent transform for the local player's hand cards (e.g. a HorizontalLayoutGroup).")]
    [SerializeField] private Transform handContainer;

    // ─── Server-only state ────────────────────────────────────────────
    private List<CardData> drawPile = new();
    private List<CardData> discardPile = new();
    private Dictionary<ulong, List<CardData>> playerHands = new();

    // ─── Synced state ─────────────────────────────────────────────────
    /// <summary>The face-up card on the discard pile, visible to everyone.</summary>
    public NetworkVariable<CardData> TopDiscard = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ─── Local client state ───────────────────────────────────────────
    /// <summary>The local player's hand (populated via ClientRpc).</summary>
    public List<CardData> LocalHand { get; private set; } = new();

    /// <summary>Currently instantiated card GameObjects in the hand.</summary>
    private readonly List<GameObject> handCardObjects = new();

    // ─── Lifecycle ────────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        TopDiscard.OnValueChanged += HandleTopDiscardChanged;
    }

    public override void OnNetworkDespawn()
    {
        TopDiscard.OnValueChanged -= HandleTopDiscardChanged;
    }

    private void HandleTopDiscardChanged(CardData oldValue, CardData newValue)
    {
        Debug.Log($"CardManager: Top discard changed to {newValue}");
        OnTopDiscardChanged?.Invoke(newValue);
    }

    // ─── Public API (called by GameManager on server/host) ────────────

    /// <summary>
    /// Builds, shuffles, and deals the deck. Call once when all players are ready.
    /// Must be called on the server/host only.
    /// </summary>
    public void StartDealing()
    {
        if (!IsServer)
        {
            Debug.LogWarning("CardManager.StartDealing() called on a client — ignored.");
            return;
        }

        BuildDeck();
        ShuffleDeck();
        DealCards();
        FlipFirstDiscard();

        Debug.Log($"CardManager: Dealing complete. Draw pile has {drawPile.Count} cards remaining.");
    }

    /// <summary>
    /// Draws one card from the draw pile and gives it to the specified player.
    /// Call on the server only.
    /// </summary>
    public CardData DrawCard(ulong clientId)
    {
        if (!IsServer)
        {
            Debug.LogWarning("CardManager.DrawCard() called on a client — ignored.");
            return default;
        }

        if (drawPile.Count == 0)
        {
            ReshuffleDiscardIntoDraw();
        }

        if (drawPile.Count == 0)
        {
            Debug.LogError("CardManager: No cards left to draw even after reshuffle!");
            return default;
        }

        CardData card = drawPile[0];
        drawPile.RemoveAt(0);

        if (!playerHands.ContainsKey(clientId))
            playerHands[clientId] = new List<CardData>();

        playerHands[clientId].Add(card);

        // Send updated hand to that player
        SendHandToClient(clientId);

        // Update card count on the player's NetworkPlayerController
        UpdatePlayerCardCount(clientId);

        Debug.Log($"CardManager: Player {clientId} drew {card}. Hand size: {playerHands[clientId].Count}");
        return card;
    }

    /// <summary>
    /// Returns how many cards a player holds. For server use.
    /// </summary>
    public int GetHandCount(ulong clientId)
    {
        if (playerHands.TryGetValue(clientId, out var hand))
            return hand.Count;
        return 0;
    }

    // ─── Deck Building ────────────────────────────────────────────────

    private void BuildDeck()
    {
        drawPile.Clear();

        CardColor[] colors = { CardColor.Red, CardColor.Yellow, CardColor.Green, CardColor.Blue };

        foreach (var color in colors)
        {
            // One Zero per color
            drawPile.Add(new CardData(color, CardValue.Zero));

            // Two each of 1–9, Skip, Reverse, DrawTwo
            for (int i = 0; i < 2; i++)
            {
                drawPile.Add(new CardData(color, CardValue.One));
                drawPile.Add(new CardData(color, CardValue.Two));
                drawPile.Add(new CardData(color, CardValue.Three));
                drawPile.Add(new CardData(color, CardValue.Four));
                drawPile.Add(new CardData(color, CardValue.Five));
                drawPile.Add(new CardData(color, CardValue.Six));
                drawPile.Add(new CardData(color, CardValue.Seven));
                drawPile.Add(new CardData(color, CardValue.Eight));
                drawPile.Add(new CardData(color, CardValue.Nine));
                drawPile.Add(new CardData(color, CardValue.Skip));
                drawPile.Add(new CardData(color, CardValue.Reverse));
                drawPile.Add(new CardData(color, CardValue.DrawTwo));
            }
        }

        // Four Wild and four Wild Draw Four
        for (int i = 0; i < 4; i++)
        {
            drawPile.Add(new CardData(CardColor.Wild, CardValue.WildCard));
            drawPile.Add(new CardData(CardColor.Wild, CardValue.WildDrawFour));
        }

        Debug.Log($"CardManager: Deck built with {drawPile.Count} cards.");
    }

    // ─── Shuffling ────────────────────────────────────────────────────

    private void ShuffleDeck()
    {
        // Fisher-Yates shuffle
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
        }

        Debug.Log("CardManager: Deck shuffled.");
    }

    // ─── Dealing ──────────────────────────────────────────────────────

    private void DealCards()
    {
        playerHands.Clear();

        var allPlayers = FindObjectsOfType<NetworkPlayerController>();
        int cardsPerPlayer = 7;

        // Initialize hands
        foreach (var player in allPlayers)
        {
            playerHands[player.OwnerClientId] = new List<CardData>();
        }

        // Deal one card at a time in round-robin (like real dealing)
        for (int card = 0; card < cardsPerPlayer; card++)
        {
            foreach (var player in allPlayers)
            {
                if (drawPile.Count == 0)
                {
                    Debug.LogError("CardManager: Ran out of cards while dealing!");
                    return;
                }

                CardData drawn = drawPile[0];
                drawPile.RemoveAt(0);
                playerHands[player.OwnerClientId].Add(drawn);
            }
        }

        // Send each player their hand and update card counts
        foreach (var player in allPlayers)
        {
            ulong clientId = player.OwnerClientId;
            SendHandToClient(clientId);
            UpdatePlayerCardCount(clientId);

            Debug.Log($"CardManager: Dealt {playerHands[clientId].Count} cards to player {clientId}.");
        }
    }

    // ─── Discard Pile ─────────────────────────────────────────────────

    private void FlipFirstDiscard()
    {
        // Keep flipping until we get a card that isn't Wild Draw Four (per Uno rules)
        while (drawPile.Count > 0)
        {
            CardData topCard = drawPile[0];
            drawPile.RemoveAt(0);

            if (topCard.Value == CardValue.WildDrawFour)
            {
                // Put it back somewhere in the deck and reshuffle
                drawPile.Add(topCard);
                ShuffleDeck();
                continue;
            }

            discardPile.Add(topCard);
            TopDiscard.Value = topCard;
            Debug.Log($"CardManager: First discard card is {topCard}.");
            return;
        }

        Debug.LogError("CardManager: Could not find a valid first discard card!");
    }

    /// <summary>
    /// Reshuffles the discard pile (except the top card) back into the draw pile.
    /// </summary>
    private void ReshuffleDiscardIntoDraw()
    {
        if (discardPile.Count <= 1)
        {
            Debug.LogWarning("CardManager: Not enough cards in discard pile to reshuffle.");
            return;
        }

        CardData topCard = discardPile[^1]; // Keep the top card
        discardPile.RemoveAt(discardPile.Count - 1);

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        discardPile.Add(topCard);

        ShuffleDeck();
        Debug.Log($"CardManager: Reshuffled discard pile into draw pile. {drawPile.Count} cards available.");
    }

    // ─── Networking ───────────────────────────────────────────────────

    private void SendHandToClient(ulong clientId)
    {
        CardData[] handArray = playerHands[clientId].ToArray();

        // Build targeted ClientRpcParams — send only to this specific client
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        ReceiveHandClientRpc(handArray, clientRpcParams);
    }

    /// <summary>
    /// Targeted ClientRpc — each player receives only their own hand.
    /// </summary>
    [ClientRpc]
    private void ReceiveHandClientRpc(CardData[] hand, ClientRpcParams clientRpcParams = default)
    {
        LocalHand.Clear();
        LocalHand.AddRange(hand);

        Debug.Log($"CardManager: Received hand with {LocalHand.Count} cards.");
        foreach (var card in LocalHand)
        {
            Debug.Log($"  - {card}");
        }

        // Spawn the visual card objects for this player's hand
        SpawnHandVisuals();

        OnLocalHandUpdated?.Invoke(LocalHand);
    }

    // ─── Visual Spawning ──────────────────────────────────────────────

    /// <summary>
    /// Destroys existing hand card objects and re-creates them from LocalHand.
    /// Runs on the local client only.
    /// </summary>
    private void SpawnHandVisuals()
    {
        ClearHandVisuals();

        if (cardPrefab == null)
        {
            Debug.LogError("CardManager: cardPrefab is not assigned!");
            return;
        }
        if (handContainer == null)
        {
            Debug.LogError("CardManager: handContainer is not assigned!");
            return;
        }
        if (cardSpriteDatabase == null)
        {
            Debug.LogError("CardManager: cardSpriteDatabase is not assigned!");
            return;
        }

        foreach (var cardData in LocalHand)
        {
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            cardObj.SetActive(true);

            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(cardData, cardSpriteDatabase);
            }
            else
            {
                Debug.LogWarning("CardManager: Card prefab is missing a CardUI component!");
            }

            handCardObjects.Add(cardObj);
        }

        Debug.Log($"CardManager: Spawned {handCardObjects.Count} card visuals.");
    }

    /// <summary>
    /// Destroys all currently instantiated hand card objects.
    /// </summary>
    private void ClearHandVisuals()
    {
        foreach (var obj in handCardObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        handCardObjects.Clear();
    }

    // ─── Card Count Sync ──────────────────────────────────────────────

    private void UpdatePlayerCardCount(ulong clientId)
    {
        var allPlayers = FindObjectsOfType<NetworkPlayerController>();
        foreach (var player in allPlayers)
        {
            if (player.OwnerClientId == clientId)
            {
                int count = playerHands[clientId].Count;
                player.SetCardCount(count);
                return;
            }
        }
    }
}
