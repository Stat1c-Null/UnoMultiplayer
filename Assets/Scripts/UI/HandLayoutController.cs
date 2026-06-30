using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Replaces HorizontalLayoutGroup on the hand container.
/// Spaces cards evenly when there is room; compresses overlap as cards approach the
/// container edge so they never exceed the available width.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class HandLayoutController : MonoBehaviour
{
    [Tooltip("Width of one card in pixels. Must match the card prefab's RectTransform width.")]
    [SerializeField] private float cardWidth = 80f;

    [Tooltip("Gap between cards when there is plenty of room.")]
    [SerializeField] private float preferredSpacing = 10f;

    private RectTransform rect;
    private readonly List<RectTransform> cards = new();

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        RefreshCardList();
        LayoutCards();
    }

    private void RefreshCardList()
    {
        cards.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.GetComponent<CardUI>() != null)
                cards.Add((RectTransform)child);
        }
    }

    private void LayoutCards()
    {
        int n = cards.Count;
        if (n == 0) return;

        float availableWidth = rect.rect.width;
        float preferredStep = cardWidth + preferredSpacing;
        float totalPreferred = cardWidth + (n - 1) * preferredStep;

        float step;
        if (totalPreferred <= availableWidth)
            step = preferredStep;
        else if (n > 1)
            step = Mathf.Max((availableWidth - cardWidth) / (n - 1), 0f);
        else
            step = 0f;

        float totalWidth = cardWidth + (n - 1) * step;
        float startX = rect.rect.center.x - totalWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < n; i++)
        {
            var cardRect = cards[i];
            cardRect.localPosition = new Vector3(
                startX + i * step,
                cardRect.localPosition.y,
                cardRect.localPosition.z
            );
        }
    }
}
