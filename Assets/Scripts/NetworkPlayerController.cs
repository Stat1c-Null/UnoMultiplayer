using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkPlayerController : NetworkBehaviour
{
  private NetworkManager networkManager;

  private void Start()
  {
    networkManager = NetworkManager.Singleton;
    // Listen for client disconnects
    networkManager.OnClientDisconnectCallback += OnClientDisconnected;
  }

  private void OnDestroy()
  {
    if (networkManager != null)
    {
      networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }
  }

  // Called when ANY client disconnects (including host)
  private void OnClientDisconnected(ulong clientId)
  {
    // If the one who disconnected is the HOST (server)
    if (clientId == NetworkManager.ServerClientId)
    {
      Debug.Log("Host has left the session. Returning to main menu...");

    }

    // Force all clients back to the main menu scene
    if (!networkManager.IsHost) // only run this on clients
    {
      Debug.Log("Returning to main menu...");
      NetworkManager.Singleton.Shutdown();
      SceneManager.LoadScene(
          "SampleScene", 
          UnityEngine.SceneManagement.LoadSceneMode.Single
      );
    }
  }

    public void LeaveSession()
    {
      if (networkManager.IsHost)
      {
        networkManager.Shutdown();
      }
      else
      {
        networkManager.Shutdown();
      }
    }
}
