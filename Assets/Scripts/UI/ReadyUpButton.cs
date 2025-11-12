using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Diagnostics;

public class ReadyUpButton : MonoBehaviour
{
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnReadyClicked);
    }

    private void OnReadyClicked()
    {
        // Find the local player
        foreach (var player in FindObjectsOfType<NetworkPlayerController>())
        {
            if (player.IsOwner) // Only local player owns their player object
            {
                // Toggle ready state through the ServerRpc
                player.SetReadyServerRpc(!player.IsReady.Value);
                return;
            }
        }
    }
}