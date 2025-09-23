using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkPlayerController : NetworkBehaviour
{
  public GameObject CreateSession;
  public GameObject JoinSession;
  private NetworkManager networkManager;

  private void Start()
  {
    //CreateSession = GameObject.FindWithTag("CreateSession");
    //JoinSession = GameObject.FindWithTag("JoinSession");
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
      CreateSession.SetActive(true);
      JoinSession.SetActive(true); 
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
        CreateSession.SetActive(true);
        JoinSession.SetActive(true); 
      }
    }
}
