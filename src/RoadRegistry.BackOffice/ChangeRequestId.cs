namespace RoadRegistry.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public readonly struct ChangeRequestId : IEquatable<ChangeRequestId>
    {
        public const int ExactLength = 32;
        public const int ExactStringLength = 64;

        private readonly byte[] _value;

        public ChangeRequestId(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length != ExactLength)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"The change request identifier must be {ExactLength} bytes.");

            _value = value;
        }

        public static bool Accepts(string value)
        {
            return !string.IsNullOrEmpty(value)
                   && value.Length == ExactStringLength
                   && Enumerable.Range(0, ExactStringLength / 2).All(index =>
                       byte.TryParse(new ReadOnlySpan<char>(new [] { value[index*2], value[index*2+1] }), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)
                   );
        }

        public static ChangeRequestId FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value),
                    "The change request identifier must not be null or empty.");

            if (value.Length != ExactStringLength)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"The change request identifier must be {ExactStringLength} characters.");

            if (Enumerable.Range(0, ExactStringLength / 2).Any(index =>
                !byte.TryParse(new ReadOnlySpan<char>(new [] { value[index*2], value[index*2+1] }), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)
            ))
            {
                throw new ArgumentException(
                    "The change request identifier must consist of hexadecimal characters only.", nameof(value));
            }
            return new ChangeRequestId(Enumerable
                .Range(0, ExactStringLength / 2)
                .Select(index =>
                    byte.Parse(new ReadOnlySpan<char>(new [] { value[index*2], value[index*2+1] }), NumberStyles.HexNumber, CultureInfo.InvariantCulture))
                .ToArray());
        }

        public static ChangeRequestId FromArchiveId(ArchiveId id)
        {
            using (var hash = SHA256.Create())
            {
                return new ChangeRequestId(
                    hash.ComputeHash(
                        Encoding.UTF8.GetBytes(id.ToString())
                    )
                );
            }
        }

        public IReadOnlyCollection<byte> ToBytes() => _value;

        public bool Equals(ChangeRequestId other) => _value == other._value;
        public override bool Equals(object other) => other is ChangeRequestId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() =>
            string.Concat(_value.Select(value => value.ToString("X2"))).ToLowerInvariant();
        public static bool operator ==(ChangeRequestId left, ChangeRequestId right) => left.Equals(right);
        public static bool operator !=(ChangeRequestId left, ChangeRequestId right) => !left.Equals(right);
        public static implicit operator string(ChangeRequestId instance) => instance.ToString();
    }
}
