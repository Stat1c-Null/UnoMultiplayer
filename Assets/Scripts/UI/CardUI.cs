using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to the Card prefab. Displays the correct sprite for a given CardData
/// by looking it up in the CardSpriteDatabase.
/// 
/// Prefab structure:  Card (Image + CardUI) 
/// The Image component on this same GameObject will be set to the card's sprite.
/// </summary>
[RequireComponent(typeof(Image))]
public class CardUI : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    /// <summary>The card data this visual represents.</summary>
    public CardData Card { get; private set; }

    private Image image;
    private RectTransform rectTransform;
    private bool isHovered;
    private float baseY;

    public float speed = 10f;
    public float height = 20f;
    public Vector3 originalPosition;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        originalPosition = rectTransform.localPosition;
        baseY = originalPosition.y;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    void Update()
    {
        Vector3 current = rectTransform.localPosition;
        float targetY = isHovered ? baseY + height : baseY;
        float nextY = Mathf.Lerp(current.y, targetY, Time.deltaTime * speed);
        rectTransform.localPosition = new Vector3(current.x, nextY, current.z);

        // Keep debug/public state in sync with the card's current slot.
        originalPosition = new Vector3(current.x, baseY, current.z);
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
