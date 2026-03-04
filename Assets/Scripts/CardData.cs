using Unity.Netcode;

/// <summary>
/// The four Uno card colors, plus Wild for color-independent cards.
/// </summary>
public enum CardColor : byte
{
    Red,
    Yellow,
    Green,
    Blue,
    Wild // Used for Wild and Wild Draw Four cards
}

/// <summary>
/// All possible Uno card face values.
/// </summary>
public enum CardValue : byte
{
    Zero,
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Skip,
    Reverse,
    DrawTwo,
    WildCard,
    WildDrawFour
}

/// <summary>
/// Represents a single Uno card. Implements INetworkSerializable so it can be
/// sent over the network via RPCs and used in NetworkVariables.
/// </summary>
public struct CardData : INetworkSerializable, System.IEquatable<CardData>
{
    public CardColor Color;
    public CardValue Value;

    public CardData(CardColor color, CardValue value)
    {
        Color = color;
        Value = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Color);
        serializer.SerializeValue(ref Value);
    }

    public bool Equals(CardData other)
    {
        return Color == other.Color && Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is CardData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ((int)Color * 397) ^ (int)Value;
    }

    public override string ToString()
    {
        if (Value == CardValue.WildCard)
            return "Wild";
        if (Value == CardValue.WildDrawFour)
            return "Wild Draw Four";

        return $"{Color} {Value}";
    }

    /// <summary>
    /// Returns true if this is a Wild or Wild Draw Four card.
    /// </summary>
    public bool IsWild => Color == CardColor.Wild;

    public static bool operator ==(CardData left, CardData right) => left.Equals(right);
    public static bool operator !=(CardData left, CardData right) => !left.Equals(right);
}
