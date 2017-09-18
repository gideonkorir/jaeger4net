using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    class TagComparer : IComparer<Tag>
    {
        public static readonly TagComparer Instance = new TagComparer();

        public int Compare(Tag x, Tag y)
            => x.Key.CompareTo(y.Key);
    }
}
