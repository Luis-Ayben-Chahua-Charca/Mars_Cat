// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Tutorial;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class TutorialSequenceTests
    {
        private static readonly TutorialStep[] Steps =
        {
            TutorialStep.Cinematic, TutorialStep.MoveCamera, TutorialStep.Energy,
            TutorialStep.Oxygen, TutorialStep.Water, TutorialStep.PlantFarm,
            TutorialStep.RatFarm, TutorialStep.Recycler, TutorialStep.Done
        };

        [Test]
        public void ConstructorStartsAtCinematicWithMissionKey()
        {
            var sequence = new TutorialSequence();
            Assert.That(sequence.CurrentStep, Is.EqualTo(TutorialStep.Cinematic));
            Assert.That(sequence.MissionKey, Is.EqualTo("tutorial.cinematic.mission"));
        }

        [Test]
        public void CompleteAdvancesThroughAllStepsAndUpdatesMission()
        {
            string[] missions =
            {
                "tutorial.cinematic.mission", "tutorial.move_camera.mission",
                "tutorial.energy.mission", "tutorial.oxygen.mission", "tutorial.water.mission",
                "tutorial.plant_farm.mission", "tutorial.rat_farm.mission", "tutorial.recycler.mission",
                string.Empty
            };
            var sequence = new TutorialSequence();
            for (int i = 0; i < Steps.Length; i++)
            {
                Assert.That(sequence.CurrentStep, Is.EqualTo(Steps[i]));
                Assert.That(sequence.MissionKey, Is.EqualTo(missions[i]));
                if (i < Steps.Length - 1)
                    Assert.That(sequence.Complete(Steps[i]), Is.True);
            }
        }

        [Test]
        public void CompleteRejectsEveryOtherStepWithoutChangingStateOrEmittingEvents()
        {
            var sequence = new TutorialSequence();
            int events = 0;
            sequence.OnStepStarted += step => events++;
            sequence.OnStepCompleted += step => events++;
            for (int i = 0; i < Steps.Length - 1; i++)
            {
                foreach (TutorialStep other in Steps)
                {
                    if (other == sequence.CurrentStep)
                        continue;
                    int previousEvents = events;
                    string previousMission = sequence.MissionKey;
                    Assert.That(sequence.Complete(other), Is.False);
                    Assert.That(sequence.CurrentStep, Is.EqualTo(Steps[i]));
                    Assert.That(sequence.MissionKey, Is.EqualTo(previousMission));
                    Assert.That(events, Is.EqualTo(previousEvents));
                }
                Assert.That(sequence.Complete(Steps[i]), Is.True);
            }
        }

        [Test]
        public void EventsReportCompletedThenStartedAndExposeUpdatedState()
        {
            var sequence = new TutorialSequence();
            var events = new List<string>();
            sequence.OnStepCompleted += step =>
            {
                events.Add("completed:" + step);
                Assert.That(sequence.CurrentStep, Is.EqualTo((TutorialStep)((int)step + 1)));
            };
            sequence.OnStepStarted += step =>
            {
                events.Add("started:" + step);
                Assert.That(sequence.CurrentStep, Is.EqualTo(step));
            };
            var expected = new List<string>();
            for (int i = 0; i < Steps.Length - 1; i++)
            {
                expected.Add("completed:" + Steps[i]);
                expected.Add("started:" + Steps[i + 1]);
                sequence.Complete(Steps[i]);
            }
            Assert.That(events, Is.EqualTo(expected));
        }

        [Test]
        public void DoneIsTerminalAndDoesNotEmitMoreEvents()
        {
            var sequence = new TutorialSequence();
            for (int i = 0; i < Steps.Length - 1; i++)
                sequence.Complete(Steps[i]);
            int events = 0;
            sequence.OnStepStarted += step => events++;
            sequence.OnStepCompleted += step => events++;
            foreach (TutorialStep step in Steps)
                Assert.That(sequence.Complete(step), Is.False);
            Assert.That(sequence.CurrentStep, Is.EqualTo(TutorialStep.Done));
            Assert.That(sequence.MissionKey, Is.Empty);
            Assert.That(events, Is.Zero);
        }

        [TestCase(-1)]
        [TestCase(9)]
        [TestCase(int.MaxValue)]
        public void CompleteRejectsUnknownEnumValues(int value)
        {
            var sequence = new TutorialSequence();
            Assert.Throws<ArgumentOutOfRangeException>(() => sequence.Complete((TutorialStep)value));
            Assert.That(sequence.CurrentStep, Is.EqualTo(TutorialStep.Cinematic));
        }

        [Test]
        public void EventCallbacksCannotAdvanceTheNextStepDuringNotification()
        {
            var sequence = new TutorialSequence();
            var nestedResults = new List<bool>();
            sequence.OnStepCompleted += step => nestedResults.Add(sequence.Complete(sequence.CurrentStep));
            sequence.OnStepStarted += step => nestedResults.Add(sequence.Complete(step));
            Assert.That(sequence.Complete(TutorialStep.Cinematic), Is.True);
            Assert.That(sequence.CurrentStep, Is.EqualTo(TutorialStep.MoveCamera));
            Assert.That(nestedResults, Is.EqualTo(new[] { false, false }));
            Assert.That(sequence.Complete(TutorialStep.MoveCamera), Is.True);
            Assert.That(sequence.CurrentStep, Is.EqualTo(TutorialStep.Energy));
        }

        [Test]
        public void SeparateSequencesKeepIndependentProgress()
        {
            var first = new TutorialSequence();
            var second = new TutorialSequence();
            first.Complete(TutorialStep.Cinematic);
            Assert.That(first.CurrentStep, Is.EqualTo(TutorialStep.MoveCamera));
            Assert.That(second.CurrentStep, Is.EqualTo(TutorialStep.Cinematic));
        }
    }
}
