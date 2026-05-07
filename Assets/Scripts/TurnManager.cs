using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

/// <summary>
/// Server-authoritative turn manager for Uno.
/// Tracks current turn and advances in a fixed order.
/// </summary>
public class TurnManager : NetworkBehaviour
{
    public event Action<ulong> OnTurnChanged;

    [Header("Turn UI")]
    [SerializeField] private TextMeshProUGUI turnIndicatorText;
    [SerializeField] private Color defaultNameColor = Color.white;
    [SerializeField] private Color activeTurnColor = new Color(1f, 0.8f, 0.2f);

    public NetworkVariable<ulong> CurrentTurnClientId = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly List<ulong> turnOrder = new();
    private int currentIndex = -1;
    private readonly Dictionary<ulong, TextMeshProUGUI> playerNameTexts = new();

    public override void OnNetworkSpawn()
    {
        CurrentTurnClientId.OnValueChanged += HandleTurnChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentTurnClientId.OnValueChanged -= HandleTurnChanged;
    }

    private void HandleTurnChanged(ulong oldValue, ulong newValue)
    {
        UpdateTurnIndicator(newValue);
        UpdateNameHighlights(newValue);
        OnTurnChanged?.Invoke(newValue);
    }

    public void InitializeTurnOrder(List<ulong> clientIds)
    {
        if (!IsServer)
        {
            Debug.LogWarning("TurnManager.InitializeTurnOrder() called on client — ignored.");
            return;
        }

        if (clientIds == null || clientIds.Count == 0)
        {
            Debug.LogWarning("TurnManager: No clients available to initialize turn order.");
            return;
        }

        turnOrder.Clear();
        turnOrder.AddRange(clientIds);
        ShuffleTurnOrder();

        currentIndex = 0;
        CurrentTurnClientId.Value = turnOrder[currentIndex];
        Debug.Log($"TurnManager: First turn is client {CurrentTurnClientId.Value}.");
    }

    public void AdvanceTurn()
    {
        if (!IsServer)
        {
            Debug.LogWarning("TurnManager.AdvanceTurn() called on client — ignored.");
            return;
        }

        if (turnOrder.Count == 0)
        {
            Debug.LogWarning("TurnManager: No turn order available.");
            return;
        }

        currentIndex = (currentIndex + 1) % turnOrder.Count;
        CurrentTurnClientId.Value = turnOrder[currentIndex];
        Debug.Log($"TurnManager: Turn advanced to client {CurrentTurnClientId.Value}.");
    }

    public bool IsPlayersTurn(ulong clientId)
    {
        return CurrentTurnClientId.Value == clientId;
    }

    public bool IsLocalPlayersTurn()
    {
        if (NetworkManager.Singleton == null)
            return false;

        return CurrentTurnClientId.Value == NetworkManager.Singleton.LocalClientId;
    }

    private void ShuffleTurnOrder()
    {
        for (int i = turnOrder.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (turnOrder[i], turnOrder[j]) = (turnOrder[j], turnOrder[i]);
        }
    }

    private void UpdateTurnIndicator(ulong clientId)
    {
        if (turnIndicatorText == null || NetworkManager.Singleton == null)
            return;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            turnIndicatorText.text = "Your turn";
            return;
        }

        var playerController = FindPlayerController(clientId);
        string name = playerController != null ? playerController.GetPlayerName() : $"Player {clientId}";
        turnIndicatorText.text = $"{name}'s turn";
    }

    private void UpdateNameHighlights(ulong clientId)
    {
        foreach (var pair in playerNameTexts)
        {
            if (pair.Value != null)
            {
                pair.Value.color = pair.Key == clientId ? activeTurnColor : defaultNameColor;
            }
        }
    }

    public void RegisterPlayerNameText(ulong clientId, TextMeshProUGUI text)
    {
        if (text == null)
            return;

        playerNameTexts[clientId] = text;
        UpdateNameHighlights(CurrentTurnClientId.Value);
    }

    public void UnregisterPlayerNameText(ulong clientId)
    {
        playerNameTexts.Remove(clientId);
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
}
