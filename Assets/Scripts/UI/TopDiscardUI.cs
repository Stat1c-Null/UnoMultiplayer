using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the current top discard card in the center pile UI.
/// </summary>
[RequireComponent(typeof(Image))]
public class TopDiscardUI : MonoBehaviour
{
    [SerializeField] private CardSpriteDatabase cardSpriteDatabase;
    [SerializeField] private CardManager cardManager;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (cardManager == null)
        {
            cardManager = FindObjectOfType<CardManager>();
        }

        if (cardManager != null)
        {
            cardManager.OnTopDiscardChanged += HandleTopDiscardChanged;
            HandleTopDiscardChanged(cardManager.TopDiscard.Value);
        }
        else
        {
            Debug.LogWarning("TopDiscardUI: CardManager not found in scene.");
        }
    }

    private void OnDisable()
    {
        if (cardManager != null)
        {
            cardManager.OnTopDiscardChanged -= HandleTopDiscardChanged;
        }
    }

    private void HandleTopDiscardChanged(CardData card)
    {
        if (cardSpriteDatabase == null)
        {
            Debug.LogError("TopDiscardUI: CardSpriteDatabase is not assigned!");
            return;
        }

        Sprite sprite = cardSpriteDatabase.GetSprite(card);
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"TopDiscardUI: No sprite for {card}");
        }
    }
}
