using UnityEngine;
using UnityEngine.UI;

public class DrawCardButton : MonoBehaviour
{
    [SerializeField] private CardManager cardManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private CanvasGroup canvasGroup;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("DrawCardButton: Button component missing.");
            enabled = false;
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnDrawClicked);
        ResolveReferences();
        SubscribeTurn();
        UpdateVisibility();
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnDrawClicked);

        if (turnManager != null)
            turnManager.OnTurnChanged -= HandleTurnChanged;
    }

    private void ResolveReferences()
    {
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();
    }

    private void SubscribeTurn()
    {
        if (turnManager == null)
            return;

        turnManager.OnTurnChanged -= HandleTurnChanged;
        turnManager.OnTurnChanged += HandleTurnChanged;
    }

    private void HandleTurnChanged(ulong _)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        bool isMyTurn = turnManager != null && turnManager.IsLocalPlayersTurn();
        SetVisible(isMyTurn);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void OnDrawClicked()
    {
        if (cardManager == null)
        {
            Debug.LogWarning("DrawCardButton: CardManager not found.");
            return;
        }

        if (turnManager != null && !turnManager.IsLocalPlayersTurn())
            return;

        cardManager.RequestDrawCardServerRpc();
    }
}
