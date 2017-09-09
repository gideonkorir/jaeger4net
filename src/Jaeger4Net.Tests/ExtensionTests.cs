using System;
using Xunit;

namespace Jaeger4Net.Tests
{
    public class ExtensionTests
    {
        [Theory]
        [InlineData("pref-", "test", "pref-test")]
        [InlineData("u.", "w", "u.w")]
        public void Adding_Prefix_Appends_To_Begining(string prefix, string text, string expected)
        {
            var actual = text.WithPrefix(prefix);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a2-data", "a2-", "data")]
        [InlineData("preftest", "pre", "ftest")]
        public void Removing_Prefix_Returns_Original_String(string text, string prefix, string expected)
        {
            var actual = text.MinusPrefix(prefix);
            Assert.Equal(expected, actual);
        }
    }
}
