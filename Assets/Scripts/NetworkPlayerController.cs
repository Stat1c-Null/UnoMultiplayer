using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Diagnostics;

public class NetworkPlayerController : NetworkBehaviour
{
    private NetworkManager networkManager;

    // Player name & ready state
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private string playerName;

    private void Start()
    {
        UnityEngine.Debug.Log(GetPlayerName());
        networkManager = NetworkManager.Singleton;

        // Give player a name based on client ID
        //playerName = "Player " + OwnerClientId;

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
        SetPlayerName("Test Name");
        UnityEngine.Debug.Log(GetPlayerName());
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
        return playerName;
    }

    public string SetPlayerName(string newName)
    {
        UnityEngine.Debug.Log("Player name got changed");
        playerName = newName;
        return playerName;
    }
}
