using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;


public class UIController : NetworkBehaviour
{
  void Start()
  {
  }
  public override void OnNetworkSpawn()
  {

  }
  
  public void OnLeaveSessionClicked()
  {
    NetworkManager.Singleton.Shutdown();
    NetworkManager.Singleton.SceneManager.LoadScene(
        "SampleScene", 
        UnityEngine.SceneManagement.LoadSceneMode.Single
    );
  }

}
