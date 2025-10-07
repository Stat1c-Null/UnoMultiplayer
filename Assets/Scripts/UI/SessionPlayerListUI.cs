using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class SessionPlayerListUI : MonoBehaviour
{
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerNamePrefab;

    private Dictionary<ulong, TextMeshProUGUI> playerNameTexts = new();

    private void Awake()
    {
        // 🔹 Automatically find "Content" container under this GameObject
        playerListContainer = transform.Find("Player List ScrollView/Viewport/Content");

        if (playerListContainer == null)
        {
            Debug.LogError("Could not find Player List container in hierarchy.");
            return;
        }

        // 🔹 Automatically grab the player list item prefab (first child)
        // You can use a hidden template item in the scene or a prefab in Resources folder
        Transform listItem = playerListContainer.Find("Session Player List Item");
        if (listItem != null)
        {
            playerNamePrefab = listItem.gameObject;
            playerNamePrefab.SetActive(false); // Hide the template
        }
        else
        {
            Debug.LogError("Could not find 'Session Player List Item' under Content.");
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;
    }

    private void AddPlayer(ulong clientId)
    {
        Debug.Log("Changing player color to green");
        GameObject entry = Instantiate(playerNamePrefab, playerListContainer);
        TextMeshProUGUI text = entry.GetComponent<TextMeshProUGUI>();
        text.text = $"Player {clientId}";
        text.color = Color.white;

        playerNameTexts[clientId] = text;
    }

    private void RemovePlayer(ulong clientId)
    {
        if (playerNameTexts.TryGetValue(clientId, out var text))
        {
            Destroy(text.gameObject);
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
}
