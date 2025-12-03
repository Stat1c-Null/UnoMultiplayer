using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Diagnostics;
using Unity.Collections;

public class NetworkPlayerController : NetworkBehaviour
{
    private NetworkManager networkManager;

    // Player name & ready state
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Networked player name (server-authoritative)
    public NetworkVariable<FixedString128Bytes> PlayerName = new NetworkVariable<FixedString128Bytes>(
        new FixedString128Bytes("Player"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        networkManager = NetworkManager.Singleton;

        // If this object is owned by the local client, tell the server our chosen lobby name
        if (IsOwner)
        {
            // If LobbyManager has a chosen name, use it; otherwise fallback to client id
            string nameToSend = LobbyManager.playerName;
            if (string.IsNullOrEmpty(nameToSend)) {
                nameToSend = $"Player {OwnerClientId}";
            }
            SetPlayerNameServerRpc(nameToSend);
        }

        // Subscribe to ready-state changes
        IsReady.OnValueChanged += OnReadyChanged;

        // Subscribe to client disconnects
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        IsReady.OnValueChanged -= OnReadyChanged;
    }

    // Called when any client disconnects (including host)
    private void OnClientDisconnected(ulong clientId)
    {
        // If the one who disconnected is the HOST (server)
        if (clientId == NetworkManager.ServerClientId)
        {
            UnityEngine.Debug.Log("Host has left the session. Returning to main menu...");
        }

        // Force all clients back to main menu scene (except host)
        if (!networkManager.IsHost)
        {
            UnityEngine.Debug.Log("Returning to main menu...");
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        }
    }

    // Called when player clicks "Ready Up"
    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready)
    {
        IsReady.Value = ready;
        UnityEngine.Debug.Log(GetPlayerName());
    }

    // Owner calls this to tell server their chosen name. Server updates NetworkVariable.
    [ServerRpc]
    public void SetPlayerNameServerRpc(string newName)
    {
        PlayerName.Value = new FixedString128Bytes(newName);
    }

    // Update color of player name in UI when ready state changes
    private void OnReadyChanged(bool oldValue, bool newValue)
    {
        var ui = FindObjectOfType<SessionPlayerListUI>();
        if (ui)
        {
            ui.UpdatePlayerReadyState(OwnerClientId, newValue);
        }
    }

    // Public function for "Leave Session" button 
    public void LeaveSession()
    {
        UnityEngine.Debug.Log("Leaving session...");
        networkManager.Shutdown();
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public string GetPlayerName()
    {
        return PlayerName.Value.ToString();
    }

    // Local convenience wrapper: owner calls this to request a name change
    public void SetPlayerNameLocal(string newName)
    {
        SetPlayerNameServerRpc(newName);
    }
}
