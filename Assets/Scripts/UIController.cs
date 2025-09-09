using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;


public class UIController : NetworkBehaviour {
  

    [SerializeField] private GameObject CreateSession;
    [SerializeField] private GameObject JoinSession;
    private NetworkVariable<bool> hideUI = new NetworkVariable<bool>(true);

    public override void OnNetworkSpawn() 
    {
      CreateSession.SetActive(false);
      JoinSession.SetActive(false);

      if(IsClient && IsOwner) { //Check if this is the local player instance
        if(hideUI.Value) {
          CreateSession.SetActive(false);
          JoinSession.SetActive(false);
        } else {
          hideUI.Value = false;
        }
      }

    }

}
