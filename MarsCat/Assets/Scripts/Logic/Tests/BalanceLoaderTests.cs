using System;
using System.Collections.Generic;
using System.Globalization;
using MarsCat.Logic.Data;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class BalanceLoaderTests
    {
        private const string Header = "clave;valor;unidad;fuente\n";

        [Test]
        public void ParseUsesInvariantCultureAndPreservesSource()
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("es-PE");
                BalanceData data = BalanceLoader.Parse(Header + "test.rate;2.5;u;diseno\n");
                Assert.That(data.Get("test.rate"), Is.EqualTo(2.5f));
                Assert.That(data.Source("test.rate"), Is.EqualTo("diseno"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Test]
        public void ParseIgnoresCommentsAndEmptyLines()
        {
            BalanceData data = BalanceLoader.Parse("\uFEFF" + Header +
                "\n  # comentario\n test.value ; 7 ; u ; archivo_origen \n");
            Assert.That(data.Get("test.value"), Is.EqualTo(7f));
            Assert.That(data.Source("test.value"), Is.EqualTo("archivo_origen"));
            Assert.That(data.TryGet("missing", out float value), Is.False);
            Assert.That(value, Is.Zero);
        }

        [Test]
        public void DuplicateKeyReportsPhysicalLineNumber()
        {
            var error = Assert.Throws<FormatException>(() => BalanceLoader.Parse(
                Header + "test.value;1;u;diseno\n# comentario\n\ntest.value;2;u;diseno"));
            Assert.That(error.Message, Does.Contain("5"));
            Assert.That(error.Message, Does.Contain("test.value"));
        }

        [TestCase("texto")]
        [TestCase("1,5")]
        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("1e100")]
        [TestCase("")]
        public void InvalidNumberReportsLineNumber(string number)
        {
            var error = Assert.Throws<FormatException>(() =>
                BalanceLoader.Parse(Header + $"test.value;{number};u;diseno"));
            Assert.That(error.Message, Does.Contain("2"));
        }

        [TestCase("test.value;1;u;")]
        [TestCase(";1;u;diseno")]
        [TestCase("test.value;1;u")]
        [TestCase("test.value;1;u;diseno;extra")]
        public void InvalidRowReportsLineNumber(string row)
        {
            var error = Assert.Throws<FormatException>(() => BalanceLoader.Parse(Header + row));
            Assert.That(error.Message, Does.Contain("2"));
        }

        [TestCase("")]
        [TestCase("clave,valor,unidad,fuente")]
        public void InvalidHeaderReportsFirstLine(string csv)
        {
            var error = Assert.Throws<FormatException>(() => BalanceLoader.Parse(csv));
            Assert.That(error.Message, Does.Contain("1"));
        }

        [Test]
        public void NullInputIsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => BalanceLoader.Parse(null));
        }

        [Test]
        public void MissingKeyIncludesItsName()
        {
            BalanceData data = BalanceLoader.Parse(Header);
            Assert.That(Assert.Throws<KeyNotFoundException>(() => data.Get("missing.key")).Message,
                Does.Contain("missing.key"));
            Assert.That(Assert.Throws<KeyNotFoundException>(() => data.Source("missing.key")).Message,
                Does.Contain("missing.key"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void InvalidLookupKeyIsRejected(string key)
        {
            BalanceData data = BalanceLoader.Parse(Header);
            Assert.Throws<ArgumentException>(() => data.Get(key));
            Assert.Throws<ArgumentException>(() => data.Source(key));
            Assert.Throws<ArgumentException>(() => data.TryGet(key, out _));
        }
    }
}
