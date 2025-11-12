using Unity.Netcode;
using UnityEngine;
using Unity.Services.Multiplayer;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public string gameSceneName = "GameRoom";
    public string playerName;

    public void OnCreateSessionClicked()
    {
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
