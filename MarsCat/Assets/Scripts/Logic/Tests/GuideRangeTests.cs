// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class GuideRangeTests
    {
        // Abono de prueba: guía 20, rango 0 a 30, meseta del 10 % de la tolerancia.
        private static GuideRange Compost()
        {
            return new GuideRange("compost", 20f, 0f, 30f, 0.1f);
        }

        [TestCase(20f, 1f)]
        [TestCase(10f, 0.5f)]
        [TestCase(25f, 0.5f)]
        [TestCase(0f, 0f)]
        [TestCase(30f, 0f)]
        public void Evaluate_ScoresDistanceToGuideOnEachSide(float value, float expected)
        {
            StepScore score = Compost().Evaluate(value);
            Assert.That(score.InRange, Is.True);
            Assert.That(score.Score, Is.EqualTo(expected).Within(1e-5f));
            Assert.That(score.ReasonKey, Is.Empty);
        }

        [Test]
        public void Evaluate_UsesDifferentToleranceBelowAndAbove()
        {
            // Misma distancia (5) a cada lado: abajo la tolerancia es 20, arriba 10.
            Assert.That(Compost().Evaluate(15f).Score, Is.EqualTo(0.75f).Within(1e-5f));
            Assert.That(Compost().Evaluate(25f).Score, Is.EqualTo(0.5f).Within(1e-5f));
        }

        [TestCase(-0.1f, "prep.compost.too_low")]
        [TestCase(30.1f, "prep.compost.too_high")]
        public void Evaluate_OutsideRangeHasNoScoreAndNamesTheFix(float value, string key)
        {
            StepScore score = Compost().Evaluate(value);
            Assert.That(score.InRange, Is.False);
            Assert.That(score.ReasonKey, Is.EqualTo(key));
        }

        [TestCase(18f)]
        [TestCase(19.5f)]
        [TestCase(21f)]
        public void Evaluate_InsidePlateauScoresOne(float value)
        {
            // Abajo la meseta es 2 (10 % de 20); arriba, 1 (10 % de 10).
            Assert.That(Compost().Evaluate(value).Score, Is.EqualTo(1f));
        }

        [Test]
        public void Evaluate_JustOutsidePlateauScoresBelowOne()
        {
            Assert.That(Compost().Evaluate(21.5f).Score, Is.EqualTo(0.85f).Within(1e-5f));
            Assert.That(Compost().Evaluate(17f).Score, Is.EqualTo(0.85f).Within(1e-5f));
        }

        [Test]
        public void Evaluate_ZeroToleranceBelowMakesFallingShortOutOfRange()
        {
            var range = new GuideRange("wash", 1f, 1f, 2f, 0.1f);
            Assert.That(range.Evaluate(0.99f).InRange, Is.False);
            Assert.That(range.Evaluate(0.99f).ReasonKey, Is.EqualTo("prep.wash.too_low"));
            Assert.That(range.Evaluate(1f).InRange, Is.True);
            Assert.That(range.Evaluate(1f).Score, Is.EqualTo(1f));
            Assert.That(range.Evaluate(1.5f).Score, Is.EqualTo(0.5f).Within(1e-5f));
        }

        [Test]
        public void Evaluate_ZeroPlateauDemandsTheExactGuide()
        {
            var range = new GuideRange("x", 10f, 0f, 20f, 0f);
            Assert.That(range.Evaluate(10f).Score, Is.EqualTo(1f));
            Assert.That(range.Evaluate(11f).Score, Is.EqualTo(0.9f).Within(1e-5f));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Evaluate_RejectsNonFiniteValues(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Compost().Evaluate(value));
        }

        [Test]
        public void Constructor_RejectsInvalidArguments()
        {
            Assert.Throws<ArgumentException>(() => new GuideRange(null, 1f, 0f, 2f, 0.1f));
            Assert.Throws<ArgumentException>(() => new GuideRange(" ", 1f, 0f, 2f, 0.1f));
            Assert.Throws<ArgumentException>(() => new GuideRange("x", 5f, 6f, 8f, 0.1f));
            Assert.Throws<ArgumentException>(() => new GuideRange("x", 9f, 6f, 8f, 0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GuideRange("x", float.NaN, 0f, 2f, 0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GuideRange("x", 1f, 0f, 2f, -0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GuideRange("x", 1f, 0f, 2f, 1f));
        }

        [Test]
        public void StepScore_FactoriesValidateTheirArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => StepScore.Scored(1.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => StepScore.Scored(-0.1f));
            Assert.Throws<ArgumentException>(() => StepScore.OutOfRange(""));
        }
    }
}
