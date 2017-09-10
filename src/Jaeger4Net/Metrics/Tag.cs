using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    public struct Tag : IEquatable<Tag>
    {
        public string Key { get; }
        public string Value { get; }

        public Tag(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public static Tag Of(string key, string value)
        {
            return new Tag(key, value);
        }

        //2 tags with same name are the same thing irrespective of the value
        public bool Equals(Tag other)
            => string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (obj is Tag tag)
                return Equals(tag);
            return false;
        }

        /// <summary>
        /// We compare equality only on <seealso cref="Key"/> therefore
        /// hashcode is only that of <seealso cref="Key"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => Key.GetHashCode();
    }
}
