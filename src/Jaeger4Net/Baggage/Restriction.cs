    using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Baggage
{
    /// <summary>
    /// Reply to check if baggage is allowed and the max value length
    /// </summary>
    public struct Restriction : IEquatable<Restriction>
    {
        public static readonly Restriction Invalid = new Restriction(false, 0);
        public static readonly Restriction Valid = new Restriction(true, BaggageConstants.DefaultMaxValueLength);

        public bool KeyAllowed { get; }
        public int MaxValueLength { get; }

        public Restriction(bool keyAllowed, int maxValueLength)
        {
            KeyAllowed = keyAllowed;
            MaxValueLength = maxValueLength;
        }

        public bool Equals(Restriction other)
            => KeyAllowed == other.KeyAllowed && MaxValueLength == other.MaxValueLength;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (obj is Restriction other)
                return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return MaxValueLength.GetHashCode() + KeyAllowed.GetHashCode();
        }
    }
}
