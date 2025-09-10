using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;


public class UIController : NetworkBehaviour {
  

  private GameObject CreateSession;
  private GameObject JoinSession;
  private NetworkVariable<bool> hideUI = new NetworkVariable<bool>(true);

  void Start()
  {
    CreateSession = GameObject.FindWithTag("CreateSession");
    JoinSession = GameObject.FindWithTag("JoinSession");
  }
  public override void OnNetworkSpawn()
  {
    CreateSession.SetActive(false);
    JoinSession.SetActive(false);

    if (IsClient && IsOwner)
    { //Check if this is the local player instance
      if (hideUI.Value)
      {
        CreateSession.SetActive(false);
        JoinSession.SetActive(false);
      }
      else
      {
        hideUI.Value = false;
      }
    }

  }

}
