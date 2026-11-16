// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Recycling;
using MarsCat.Logic.Resources;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class CompostMixTests
    {
        private static ResourcePool Pool(float cat, float rat, float crop)
        {
            var amounts = ResourceConfigTests.FiniteValues(0f);
            amounts[ResourceId.CatWaste] = cat;
            amounts[ResourceId.RatWaste] = rat;
            amounts[ResourceId.CropResidue] = crop;
            return new ResourcePool(new ResourceConfig(amounts, ResourceConfigTests.FiniteValues(100f)));
        }

        // Gato 10, cama de rata 75, restos 20; guía 30, rango 20 a 40; mismo nitrógeno por kilo.
        private static CompostMixConfig Config()
        {
            return new CompostMixConfig(10f, 1f, 75f, 1f, 20f, 1f, 30f, 20f, 40f, 0.1f);
        }

        private static CompostMix Mix(ResourcePool pool, float cat, float rat, float crop)
        {
            var mix = new CompostMix(Config(), pool);
            mix.SetAmount(ResourceId.CatWaste, cat);
            mix.SetAmount(ResourceId.RatWaste, rat);
            mix.SetAmount(ResourceId.CropResidue, crop);
            return mix;
        }

        [Test]
        public void OnlyCatWasteGivesALowRatioOutOfRange()
        {
            var mix = Mix(Pool(10f, 10f, 10f), 3f, 0f, 0f);
            Assert.That(mix.Ratio, Is.EqualTo(10f).Within(1e-4f));
            StepScore score = mix.Evaluate();
            Assert.That(score.InRange, Is.False);
            Assert.That(score.ReasonKey, Is.EqualTo("prep.compost.too_low"));
        }

        [Test]
        public void CatWasteAndCropResidueAloneStayBelowTheRange()
        {
            var mix = Mix(Pool(10f, 10f, 10f), 3f, 0f, 3f);
            Assert.That(mix.Ratio, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(mix.Evaluate().InRange, Is.False);
        }

        [Test]
        public void AddingRatBeddingRaisesTheRatioToTheGuide()
        {
            var mix = Mix(Pool(10f, 10f, 10f), 3f, 0f, 3f);
            float before = mix.Ratio;
            mix.SetAmount(ResourceId.RatWaste, 2f);
            Assert.That(mix.Ratio, Is.GreaterThan(before));
            Assert.That(mix.Ratio, Is.EqualTo(30f).Within(1e-4f));
            StepScore score = mix.Evaluate();
            Assert.That(score.InRange, Is.True);
            Assert.That(score.Score, Is.EqualTo(1f));
        }

        [Test]
        public void OnlyRatBeddingIsTooMuchCarbon()
        {
            var mix = Mix(Pool(10f, 10f, 10f), 0f, 2f, 0f);
            StepScore score = mix.Evaluate();
            Assert.That(score.InRange, Is.False);
            Assert.That(score.ReasonKey, Is.EqualTo("prep.compost.too_high"));
        }

        [Test]
        public void ScoreFallsAsTheRatioLeavesTheGuide()
        {
            // 10 de gato y 3 de cama: (100 + 225) / 13 = 25, a medio camino de la guía.
            var mix = Mix(Pool(20f, 20f, 20f), 10f, 3f, 0f);
            Assert.That(mix.Ratio, Is.EqualTo(25f).Within(1e-4f));
            Assert.That(mix.Evaluate().Score, Is.EqualTo(0.5f).Within(1e-4f));
        }

        [Test]
        public void RatioIsCarbonOverNitrogenNotTheAverageOfTheRatios()
        {
            // Gato con el doble de nitrógeno por kilo: 1 kg de gato (N=2, C=20) + 1 kg de cama (N=1, C=75).
            var config = new CompostMixConfig(10f, 2f, 75f, 1f, 20f, 1f, 30f, 20f, 40f, 0.1f);
            var mix = new CompostMix(config, Pool(5f, 5f, 5f));
            mix.SetAmount(ResourceId.CatWaste, 1f);
            mix.SetAmount(ResourceId.RatWaste, 1f);
            Assert.That(mix.Ratio, Is.EqualTo(95f / 3f).Within(1e-4f));
        }

        [Test]
        public void TryConfirm_InRangeChargesTheWasteAndKeepsTheScore()
        {
            ResourcePool pool = Pool(10f, 10f, 10f);
            var mix = Mix(pool, 3f, 2f, 3f);
            Assert.That(mix.TryConfirm(), Is.True);
            Assert.That(pool.Get(ResourceId.CatWaste), Is.EqualTo(7f));
            Assert.That(pool.Get(ResourceId.RatWaste), Is.EqualTo(8f));
            Assert.That(pool.Get(ResourceId.CropResidue), Is.EqualTo(7f));
            Assert.That(mix.ConfirmedScore, Is.EqualTo(1f));
            Assert.That(mix.LastReasonKey, Is.Empty);
        }

        [Test]
        public void TryConfirm_OutOfRangeDoesNotCharge()
        {
            ResourcePool pool = Pool(10f, 10f, 10f);
            var mix = Mix(pool, 3f, 0f, 0f);
            Assert.That(mix.TryConfirm(), Is.False);
            Assert.That(pool.Get(ResourceId.CatWaste), Is.EqualTo(10f));
            Assert.That(mix.LastReasonKey, Is.EqualTo("prep.compost.too_low"));
            Assert.That(mix.ConfirmedScore, Is.Zero);
        }

        [Test]
        public void TryConfirm_WithoutEnoughWasteDoesNotChargeAnything()
        {
            ResourcePool pool = Pool(10f, 1f, 10f);
            var mix = Mix(pool, 3f, 2f, 3f);
            Assert.That(mix.TryConfirm(), Is.False);
            Assert.That(mix.LastReasonKey, Is.EqualTo("prep.compost.insufficient"));
            Assert.That(pool.Get(ResourceId.CatWaste), Is.EqualTo(10f));
            Assert.That(pool.Get(ResourceId.RatWaste), Is.EqualTo(1f));
            Assert.That(pool.Get(ResourceId.CropResidue), Is.EqualTo(10f));
        }

        [Test]
        public void EmptyMixIsOutOfRangeWithItsOwnKey()
        {
            var mix = new CompostMix(Config(), Pool(1f, 1f, 1f));
            Assert.That(mix.Ratio, Is.Zero);
            StepScore score = mix.Evaluate();
            Assert.That(score.InRange, Is.False);
            Assert.That(score.ReasonKey, Is.EqualTo("prep.compost.empty"));
        }

        [Test]
        public void WorksAsAStepOfAMultiStepPreparation()
        {
            ResourcePool pool = Pool(10f, 10f, 10f);
            var prep = new MultiStepPreparation(
                new IPreparationStep[] { Mix(pool, 3f, 2f, 3f) }, new YieldConfig(0.8f, 1.2f, 0.1f, 1f));
            Assert.That(prep.Confirm().Yield, Is.EqualTo(1.2f).Within(1e-5f));
            Assert.That(pool.Get(ResourceId.CatWaste), Is.EqualTo(7f));
        }

        [Test]
        public void SetAmount_RejectsOtherResourcesAndInvalidAmounts()
        {
            var mix = new CompostMix(Config(), Pool(1f, 1f, 1f));
            Assert.Throws<ArgumentException>(() => mix.SetAmount(ResourceId.Water, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => mix.SetAmount(ResourceId.CatWaste, -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => mix.SetAmount(ResourceId.CatWaste, float.NaN));
            Assert.Throws<ArgumentException>(() => mix.GetAmount(ResourceId.Metal));
            mix.SetAmount(ResourceId.RatWaste, 2f);
            Assert.That(mix.GetAmount(ResourceId.RatWaste), Is.EqualTo(2f));
        }

        [Test]
        public void Constructor_RejectsNulls()
        {
            Assert.Throws<ArgumentNullException>(() => new CompostMix(null, Pool(1f, 1f, 1f)));
            Assert.Throws<ArgumentNullException>(() => new CompostMix(Config(), null));
        }
    }
}
