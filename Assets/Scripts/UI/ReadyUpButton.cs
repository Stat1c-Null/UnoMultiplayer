using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ReadyUpButton : MonoBehaviour
{
    private Button button;
    private NetworkPlayerController localPlayer;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnReadyClicked);
    }

    public void OnReadyClicked()
    {
        Debug.Log("Ready button clicked");
        if (localPlayer == null)
        {
            foreach (var obj in FindObjectsOfType<NetworkPlayerController>())
            {
                if (obj.IsOwner)
                {
                    localPlayer = obj;
                    break;
                }
            }
        }

        if (localPlayer != null)
        {
            bool newState = !localPlayer.IsReady.Value;
            localPlayer.SetReadyServerRpc(newState);
        }
    }
}
