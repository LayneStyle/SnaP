using System;
using Unity.Netcode;

/// <summary>
/// Source https://github.com/ccqi/TexasHoldem
/// </summary>
[Serializable]
public class CardObject : INetworkSerializable, IEquatable<CardObject> // <-- AÑADE IEquatable AQUÍ
{
    public Suit Suit => _suit;
    private Suit _suit;

    public Value Value => _value;
    private Value _value;

    public CardObject() { }
    
    public CardObject(Suit suit, Value value)
    {
        _suit = suit;
        _value = value; 
    }
    
    public static string RankToString(int rank)
    {
        return ((Value)rank).ToString();
    }
    
    public int GetRank()
    {
        return (int)Value;
    }

    // AÑADE ESTE MÉTODO PÚBLICO
    public bool Equals(CardObject other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Suit == other.Suit && Value == other.Value;
    }

    #region Overrides

    public override string ToString()
    {
        return $"{_value} of {_suit}";
    }

    public static bool operator ==(CardObject a, CardObject b)
    {
        return a.Value == b.Value;
    }
    
    public static bool operator !=(CardObject a, CardObject b)
    {
        return a.Value != b.Value;
    }
    
    public static bool operator <(CardObject a, CardObject b)
    {
        return a.Value < b.Value;
    }
    
    public static bool operator >(CardObject a, CardObject b)
    {
        return a.Value > b.Value;
    }
    
    public static bool operator <=(CardObject a, CardObject b)
    {
        return a.Value <= b.Value;
    }
    
    public static bool operator >=(CardObject a, CardObject b)
    {
        return a.Value >= b.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((CardObject)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Suit, (int)Value);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _suit);
        serializer.SerializeValue(ref _value);
    }

    #endregion
}