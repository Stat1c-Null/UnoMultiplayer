using Unity.Netcode;
using UnityEngine;
using Unity.Services.Multiplayer;

public class LobbyManager : MonoBehaviour
{
    public string gameSceneName = "GameRoom";

    public void OnCreateSessionClicked()
    {
        // Session is already created by Multiplayer Center.
        // Just wait for NGO to finish starting.
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
    }

    private void HandleServerStarted()
    {
        Debug.Log("Host started, now loading scene...");

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                gameSceneName, 
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }

        // Unsubscribe so it only runs once
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
    }
}
