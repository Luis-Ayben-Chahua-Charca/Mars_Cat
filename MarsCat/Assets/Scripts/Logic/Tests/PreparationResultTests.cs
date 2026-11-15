using System;
using MarsCat.Logic.Preparation;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class PreparationResultTests
    {
        [TestCase(0f)]
        [TestCase(1f)]
        [TestCase(1.5f)]
        public void OkPreservesYieldAndStarts(float yield)
        {
            PreparationResult result = PreparationResult.Ok(yield);
            Assert.That(result.Starts, Is.True);
            Assert.That(result.Yield, Is.EqualTo(yield));
            Assert.That(result.ReasonKey, Is.Empty);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void OkRejectsInvalidYield(float yield)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PreparationResult.Ok(yield));
        }

        [Test]
        public void FailPreservesReasonAndDoesNotStart()
        {
            PreparationResult result = PreparationResult.Fail("prep.test.out_of_range");
            Assert.That(result.Starts, Is.False);
            Assert.That(result.Yield, Is.Zero);
            Assert.That(result.ReasonKey, Is.EqualTo("prep.test.out_of_range"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void FailRejectsMissingReason(string reasonKey)
        {
            Assert.Throws<ArgumentException>(() => PreparationResult.Fail(reasonKey));
        }
    }
}
