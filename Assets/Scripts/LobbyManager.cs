using Unity.Netcode;
using UnityEngine;
using Unity.Services.Multiplayer;
using System.Collections.Generic;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public string gameSceneName = "GameRoom";
    // Instance field that can be set via the lobby UI
    public static string playerName;

    public GameObject nameInputField;

    public void Start() {
        nameInputField = GameObject.FindWithTag("PlayerNameInput");
    }

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

    public void SetName()
    {
        playerName = nameInputField.GetComponent<TextMeshPro>().text; //TODO: Fix issue on this line
    }
}
