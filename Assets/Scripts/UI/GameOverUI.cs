using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

/// <summary>
/// Shows the end-of-game panel when a player empties their hand.
/// The same panel is shown to everyone; only the result text changes
/// depending on whether the local player won or lost. The Continue
/// button shuts the session down and returns to the menu scene.
///
/// Place this on a GameObject that is always active (e.g. the Canvas)
/// and assign the (initially hidden) panel in the Inspector.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("The CardManager that raises OnGameWon. Auto-found if left empty.")]
    [SerializeField] private CardManager cardManager;

    [Header("UI")]
    [Tooltip("Root of the win/lose panel. Hidden until the game ends.")]
    [SerializeField] private GameObject panel;

    [Tooltip("Text that displays the win/lose message.")]
    [SerializeField] private TextMeshProUGUI resultText;

    [Tooltip("Button that returns everyone to the menu scene.")]
    [SerializeField] private Button continueButton;

    [Header("Messages")]
    [Tooltip("Shown to the player who won.")]
    [SerializeField] private string winText = "You Won!";

    [Tooltip("Shown to everyone else. {0} is replaced with the winner's name.")]
    [SerializeField] private string loseText = "You Lost!\n{0} won.";

    [Header("Navigation")]
    [SerializeField] private string menuSceneName = "SampleScene";

    private void Awake()
    {
        // Start hidden — only revealed when someone wins.
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        if (cardManager != null)
            cardManager.OnGameWon += HandleGameWon;
        else
            Debug.LogWarning("GameOverUI: CardManager not found in scene.");

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDisable()
    {
        if (cardManager != null)
            cardManager.OnGameWon -= HandleGameWon;

        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    private void HandleGameWon(ulong winnerClientId, string winnerName)
    {
        bool localPlayerWon = NetworkManager.Singleton != null
            && NetworkManager.Singleton.LocalClientId == winnerClientId;

        if (resultText != null)
        {
            resultText.text = localPlayerWon
                ? winText
                : string.Format(loseText, winnerName);
        }

        if (panel != null)
            panel.SetActive(true);
    }

    private void OnContinueClicked()
    {
        // Leaving the session disconnects this client. When the host leaves,
        // the disconnect callback in NetworkPlayerController returns the
        // remaining clients to the menu as well.
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }
}
