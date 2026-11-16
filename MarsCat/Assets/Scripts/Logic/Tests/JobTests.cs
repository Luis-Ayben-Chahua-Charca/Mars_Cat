// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using MarsCat.Logic.Work;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class JobTests
    {
        [Test]
        public void Constructor_RejectsMissingIdsAndUnknownEnums()
        {
            foreach (string id in new[] { null, "", " " })
            {
                Assert.Throws<ArgumentException>(() => new Job(id, JobKind.Post, "water", false));
                Assert.Throws<ArgumentException>(() => new Job("job", JobKind.Post, id, false));
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => new Job("job", (JobKind)99, "water", false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Job("job", JobKind.OneOff, "water", false,
                (TaskPriority)99));
        }
    }
}
