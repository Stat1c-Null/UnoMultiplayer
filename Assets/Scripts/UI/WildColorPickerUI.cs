using UnityEngine;

/// <summary>
/// Shows a color-picker panel when the local player plays a Wild or Wild Draw Four.
/// The panel contains 4 buttons (Red, Yellow, Green, Blue). Wire each button's
/// onClick to OnColorSelected and pass the color name as a string argument, e.g. "Red".
///
/// Place this script on an always-active object (e.g. the Canvas), not on the panel
/// itself, so it receives the OnWildCardPlayed event even while the panel is hidden.
/// </summary>
public class WildColorPickerUI : MonoBehaviour
{
    [Tooltip("Root of the color picker panel. Hidden until a Wild is played.")]
    [SerializeField] private GameObject panel;

    [Tooltip("CardManager in the scene. Auto-found if left empty.")]
    [SerializeField] private CardManager cardManager;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        if (cardManager != null)
            cardManager.OnWildCardPlayed += ShowPicker;
        else
            Debug.LogWarning("WildColorPickerUI: CardManager not found in scene.");
    }

    private void OnDisable()
    {
        if (cardManager != null)
            cardManager.OnWildCardPlayed -= ShowPicker;
    }

    private void ShowPicker()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    /// <summary>
    /// Called by each color button's onClick. Pass the button's color name
    /// as the argument — must match a CardColor enum name exactly: Red, Yellow, Green, Blue.
    /// </summary>
    public void OnColorSelected(string colorName)
    {
        if (!System.Enum.TryParse(colorName, ignoreCase: true, out CardColor color)
            || color == CardColor.Wild)
        {
            Debug.LogWarning($"WildColorPickerUI: '{colorName}' is not a valid color name.");
            return;
        }

        if (panel != null)
            panel.SetActive(false);

        if (cardManager != null)
            cardManager.SelectWildColorServerRpc(color);
    }
}
