namespace RoadRegistry.BackOffice
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    public readonly struct AttributeId : IEquatable<AttributeId>, IComparable<AttributeId>
    {
        private readonly int _value;

        public AttributeId(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The attribute identifier must be greater than or equal to zero.");
            }

            _value = value;
        }

        public static bool Accepts(int value)
        {
            return value >= 0;
        }

        public AttributeId Next()
        {
            if (_value == int.MaxValue)
            {
                throw new NotSupportedException(
                    "There is no next attribute identifier because the maximum of the integer data type has been reached.");
            }
            return new AttributeId(_value + 1);
        }

        public static AttributeId Max(AttributeId left, AttributeId right) =>
            new AttributeId(Math.Max(left._value, right._value));
        public static AttributeId Min(AttributeId left, AttributeId right) =>
            new AttributeId(Math.Min(left._value, right._value));

        [Pure]
        public int ToInt32() => _value;
        
        public bool Equals(AttributeId other) => _value == other._value;
        
        public override bool Equals(object obj) => obj is AttributeId id && Equals(id);
        
        public override int GetHashCode() => _value.GetHashCode();
        
        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
        
        public int CompareTo(AttributeId other) => _value.CompareTo(other._value);
        
        public static bool operator ==(AttributeId left, AttributeId right) => left.Equals(right);
        
        public static bool operator !=(AttributeId left, AttributeId right) => !left.Equals(right);
        
        public static implicit operator int(AttributeId instance) => instance._value;

        public static bool operator <(AttributeId left, AttributeId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(AttributeId left, AttributeId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(AttributeId left, AttributeId right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(AttributeId left, AttributeId right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
