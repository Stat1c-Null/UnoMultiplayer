using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to the Card prefab. Displays the correct sprite for a given CardData
/// by looking it up in the CardSpriteDatabase.
/// 
/// Prefab structure:  Card (Image + CardUI) 
/// The Image component on this same GameObject will be set to the card's sprite.
/// </summary>
[RequireComponent(typeof(Image))]
public class CardUI : MonoBehaviour
{
    /// <summary>The card data this visual represents.</summary>
    public CardData Card { get; private set; }

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    /// <summary>
    /// Initialises this card visual with the given data and sprite database.
    /// </summary>
    public void Setup(CardData cardData, CardSpriteDatabase spriteDatabase)
    {
        Card = cardData;

        if (image == null)
            image = GetComponent<Image>();

        Sprite sprite = spriteDatabase.GetSprite(cardData);
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"CardUI: No sprite for {cardData}");
        }
    }
}
