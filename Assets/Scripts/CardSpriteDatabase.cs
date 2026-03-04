using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that maps every Uno card (CardColor + CardValue) to its sprite.
/// Create one via Assets → Create → Uno → Card Sprite Database,
/// then drag all 108 card sprites into the entries list in the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "CardSpriteDatabase", menuName = "Uno/Card Sprite Database")]
public class CardSpriteDatabase : ScriptableObject
{
    [Serializable]
    public class CardSpriteEntry
    {
        public CardColor color;
        public CardValue value;
        public Sprite sprite;
    }

    [Header("Drag all card sprites here and set their color + value")]
    public List<CardSpriteEntry> entries = new();

    [Header("Fallback sprite if a card isn't found")]
    public Sprite cardBackSprite;

    // Runtime lookup dictionary (built on first access)
    private Dictionary<(CardColor, CardValue), Sprite> _lookup;

    /// <summary>
    /// Returns the sprite for the given card, or the card-back fallback if not found.
    /// </summary>
    public Sprite GetSprite(CardData card)
    {
        BuildLookupIfNeeded();

        if (_lookup.TryGetValue((card.Color, card.Value), out Sprite sprite))
            return sprite;

        Debug.LogWarning($"CardSpriteDatabase: No sprite found for {card}. Using fallback.");
        return cardBackSprite;
    }

    /// <summary>
    /// Returns the sprite for the given color/value combo.
    /// </summary>
    public Sprite GetSprite(CardColor color, CardValue value)
    {
        return GetSprite(new CardData(color, value));
    }

    private void BuildLookupIfNeeded()
    {
        if (_lookup != null)
            return;

        _lookup = new Dictionary<(CardColor, CardValue), Sprite>();

        foreach (var entry in entries)
        {
            var key = (entry.color, entry.value);
            if (!_lookup.TryAdd(key, entry.sprite))
            {
                Debug.LogWarning($"CardSpriteDatabase: Duplicate entry for {entry.color} {entry.value}.");
            }
        }
    }

    // Re-build the lookup if the asset is changed in the Inspector
    private void OnValidate()
    {
        _lookup = null;
    }
}
