using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkPlayerController : NetworkBehaviour
{

  //private GameObject NetworkManager;

  
  public void LeaveSession()
  {
    if (NetworkManager.Singleton.IsHost)
    {
      NetworkManager.Singleton.Shutdown();
    }
  }
}
