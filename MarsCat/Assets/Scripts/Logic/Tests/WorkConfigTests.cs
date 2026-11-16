// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using MarsCat.Logic.Work;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class WorkConfigTests
    {
        [Test]
        public void Constructor_RejectsMissingGroupsEmptyIdsAndDuplicateBuildings()
        {
            var empty = new string[0];
            Assert.Throws<ArgumentNullException>(() => new WorkConfig(null, empty, empty));
            Assert.Throws<ArgumentNullException>(() => new WorkConfig(empty, null, empty));
            Assert.Throws<ArgumentNullException>(() => new WorkConfig(empty, empty, null));
            Assert.Throws<ArgumentException>(() => new WorkConfig(new[] { " " }, empty, empty));
            Assert.Throws<ArgumentException>(() => new WorkConfig(new[] { "water", "water" }, empty, empty));
            Assert.Throws<ArgumentException>(() => new WorkConfig(new[] { "water" }, new[] { "water" }, empty));
        }

        [Test]
        public void Constructor_CopiesInputGroups()
        {
            var none = new[] { "recycler" };
            var config = new WorkConfig(none, new string[0], new string[0]);
            none[0] = "changed";
            var assigner = new JobAssigner(config);
            Assert.DoesNotThrow(() => assigner.RegisterJob(new Job("post", JobKind.Post, "recycler", false)));
        }
    }
}
