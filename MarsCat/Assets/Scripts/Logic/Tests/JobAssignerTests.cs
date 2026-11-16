// Asistencia de IA: escrito con Codex (OpenAI).
using System;
using System.Collections.Generic;
using System.Reflection;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Resources;
using MarsCat.Logic.Work;
using NUnit.Framework;

namespace MarsCat.Logic.Tests
{
    public sealed class JobAssignerTests
    {
        private static JobAssigner Assigner()
        {
            return new JobAssigner(new WorkConfig(new[] { "recycler" }, new[] { "water" }, new[] { "moxie" }));
        }

        private static CatColony Colony(int count)
        {
            var pool = new ResourcePool(new ResourceConfig(ResourceConfigTests.FiniteValues(0f),
                ResourceConfigTests.FiniteValues(1000f)));
            var colony = new CatColony(CatTests.Config(), pool, 0);
            colony.Spawn(count);
            return colony;
        }

        private static Job Post(string building)
        {
            return new Job(building, JobKind.Post, building, false);
        }

        [Test]
        public void Tick_AssignsOneOffBeforePostAndKeepsExistingWorkers()
        {
            var assigner = Assigner();
            var colony = Colony(2);
            var assignments = new List<string>();
            assigner.OnAssigned += (job, cat) => assignments.Add(job.Id);
            assigner.RegisterJob(Post("water"));
            assigner.RegisterJob(new Job("clean", JobKind.OneOff, "moxie", true));
            assigner.Tick(0f, colony, false);
            assigner.Tick(1f, colony, false);
            Assert.That(assignments, Is.EqualTo(new[] { "clean", "water" }));
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.WorkingOutdoor));
            Assert.That(colony.Cats[1].State, Is.EqualTo(CatState.WorkingIndoor));
        }

        [Test]
        public void Tick_NightReleasesOnceAndDawnReassigns()
        {
            var assigner = Assigner();
            var colony = Colony(1);
            int assigned = 0;
            int released = 0;
            assigner.OnAssigned += (job, cat) => assigned++;
            assigner.OnReleased += (job, cat) => released++;
            assigner.RegisterJob(Post("water"));
            assigner.Tick(1f, colony, true);
            Assert.That(assigned, Is.Zero);
            assigner.Tick(1f, colony, false);
            colony.Tick(0f, true);
            assigner.Tick(1f, colony, true);
            assigner.Tick(1f, colony, true);
            Assert.That(released, Is.EqualTo(1));
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.Resting));
            colony.Tick(0f, false);
            assigner.Tick(1f, colony, false);
            Assert.That(assigned, Is.EqualTo(2));
        }

        [TestCase(TaskPriority.Normal, "recycler", true)]
        [TestCase(TaskPriority.Normal, "water", false)]
        [TestCase(TaskPriority.Normal, "moxie", false)]
        [TestCase(TaskPriority.Vital, "recycler", true)]
        [TestCase(TaskPriority.Vital, "water", true)]
        [TestCase(TaskPriority.Vital, "moxie", true)]
        public void Tick_PreemptionRespectsTaskAndBuildingPriority(TaskPriority priority,
            string building, bool shouldAssign)
        {
            var assigner = Assigner();
            var colony = Colony(1);
            var events = new List<string>();
            assigner.RegisterJob(Post(building));
            assigner.Tick(0f, colony, false);
            assigner.OnReleased += (job, cat) => events.Add("release:" + job.Id);
            assigner.OnAssigned += (job, cat) => events.Add("assign:" + job.Id);
            assigner.RegisterJob(new Job("task", JobKind.OneOff, "moxie", false, priority));
            assigner.Tick(0f, colony, false);
            Assert.That(events, Is.EqualTo(shouldAssign
                ? new[] { "release:" + building, "assign:task" } : new string[0]));
        }

        [Test]
        public void Tick_VitalUsesNoneThenHighThenMaxAndDoesNotStealOneOff()
        {
            var assigner = Assigner();
            var colony = Colony(3);
            var released = new List<string>();
            assigner.OnReleased += (job, cat) => released.Add(job.Id);
            assigner.RegisterJob(Post("moxie"));
            assigner.RegisterJob(Post("water"));
            assigner.RegisterJob(Post("recycler"));
            assigner.Tick(0f, colony, false);
            for (int i = 0; i < 4; i++)
            {
                assigner.RegisterJob(new Job("rescue" + i, JobKind.OneOff, "moxie", false, TaskPriority.Vital));
                assigner.Tick(0f, colony, false);
            }
            Assert.That(released, Is.EqualTo(new[] { "recycler", "water", "moxie" }));
        }

        [Test]
        public void Tick_UsesFreeCatBeforeTakingWorkerAndVitalBeforeNormal()
        {
            var assigner = Assigner();
            var colony = Colony(2);
            var assigned = new List<string>();
            int released = 0;
            assigner.OnAssigned += (job, cat) => assigned.Add(job.Id);
            assigner.OnReleased += (job, cat) => released++;
            assigner.RegisterJob(Post("moxie"));
            assigner.Tick(0f, colony, false);
            assigner.RegisterJob(new Job("normal", JobKind.OneOff, "water", false));
            assigner.RegisterJob(new Job("vital", JobKind.OneOff, "water", false, TaskPriority.Vital));
            assigner.Tick(0f, colony, false);
            Assert.That(assigned, Is.EqualTo(new[] { "moxie", "vital" }));
            Assert.That(released, Is.Zero);
        }

        [Test]
        public void UnregisterJob_CompletesTaskAndReturnsCatToPostOnNextTick()
        {
            var assigner = Assigner();
            var colony = Colony(1);
            var assigned = new List<string>();
            int released = 0;
            assigner.OnAssigned += (job, cat) => assigned.Add(job.Id);
            assigner.OnReleased += (job, cat) => released++;
            assigner.RegisterJob(Post("recycler"));
            assigner.Tick(0f, colony, false);
            assigner.RegisterJob(new Job("clean", JobKind.OneOff, "moxie", false));
            assigner.Tick(0f, colony, false);
            assigner.UnregisterJob("clean");
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.Free));
            Assert.That(released, Is.EqualTo(2));
            assigner.Tick(0f, colony, false);
            Assert.That(assigned, Is.EqualTo(new[] { "recycler", "clean", "recycler" }));
        }

        [Test]
        public void Release_ManuallyFreesProtectedBuildingForNormalTask()
        {
            var assigner = Assigner();
            var colony = Colony(1);
            string assigned = null;
            assigner.OnAssigned += (job, cat) => assigned = job.Id;
            assigner.RegisterJob(Post("moxie"));
            assigner.Tick(0f, colony, false);
            assigner.RegisterJob(new Job("clean", JobKind.OneOff, "water", false));
            assigner.Release("moxie");
            assigner.Tick(0f, colony, false);
            Assert.That(assigned, Is.EqualTo("clean"));
        }

        [Test]
        public void Colony_AlertImmediatelyReleasesWorkerAndDeathDoesNotReleaseTwice()
        {
            var assigner = Assigner();
            var colony = Colony(1);
            int released = 0;
            assigner.OnReleased += (job, cat) => released++;
            assigner.RegisterJob(Post("water"));
            assigner.Tick(0f, colony, false);
            colony.Tick(240f, false);
            Assert.That(released, Is.EqualTo(1));
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.SeekingFood));
            assigner.Tick(0f, colony, false);
            colony.Tick(60f, false);
            assigner.Tick(0f, colony, false);
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.Dead));
            Assert.That(released, Is.EqualTo(1));
        }

        [Test]
        public void Tick_ChangingColonyReleasesOldWorker()
        {
            var assigner = Assigner();
            var oldColony = Colony(1);
            var newColony = Colony(1);
            Cat assigned = null;
            assigner.OnAssigned += (job, cat) => assigned = cat;
            assigner.RegisterJob(Post("water"));
            assigner.Tick(0f, oldColony, false);
            assigner.Tick(0f, newColony, false);
            Assert.That(oldColony.Cats[0].State, Is.EqualTo(CatState.Free));
            Assert.That(assigned, Is.SameAs(newColony.Cats[0]));
        }

        [Test]
        public void Tick_DegradedCatStaysIndoorsAndCannotBeTakenForOutdoorTask()
        {
            var assigner = Assigner();
            var colony = Colony(0);
            var degraded = new Cat(0, CatTests.Config(), isDegraded: true);
            // Spawn no admite gatos degradados: esta población es solo una fixture de prueba.
            var cats = (List<Cat>)typeof(CatColony).GetField("_cats",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(colony);
            cats.Add(degraded);
            var assigned = new List<string>();
            assigner.OnAssigned += (job, cat) => assigned.Add(job.Id);
            assigner.RegisterJob(new Job("outdoor", JobKind.Post, "water", true));
            assigner.RegisterJob(Post("recycler"));
            assigner.Tick(0f, colony, false);
            Assert.That(assigned, Is.EqualTo(new[] { "recycler" }));
            assigner.RegisterJob(new Job("rescue", JobKind.OneOff, "moxie", true, TaskPriority.Vital));
            assigner.Tick(0f, colony, false);
            Assert.That(assigned, Is.EqualTo(new[] { "recycler" }));
            Assert.That(degraded.State, Is.EqualTo(CatState.WorkingIndoor));
        }

        [Test]
        public void CatRelease_ImmediatelyVacatesPostAndWaitingTaskUsesIt()
        {
            var assigner = Assigner();
            var colony = Colony(1);
            int released = 0;
            string assigned = null;
            assigner.OnReleased += (job, cat) => released++;
            assigner.OnAssigned += (job, cat) => assigned = job.Id;
            assigner.RegisterJob(Post("moxie"));
            assigner.Tick(0f, colony, false);
            assigner.RegisterJob(new Job("clean", JobKind.OneOff, "water", false));
            assigner.Tick(0f, colony, false);
            Assert.That(assigned, Is.EqualTo("moxie"));
            colony.Cats[0].Release();
            Assert.That(released, Is.EqualTo(1));
            assigner.Tick(0f, colony, false);
            Assert.That(assigned, Is.EqualTo("clean"));
        }

        [Test]
        public void UnregisterJob_RemovesVacantPostAndReleasesOccupiedPostOnlyOnce()
        {
            var assigner = Assigner();
            var colony = Colony(1);
            int released = 0;
            assigner.OnReleased += (job, cat) => released++;
            assigner.RegisterJob(Post("water"));
            assigner.RegisterJob(Post("recycler"));
            assigner.Tick(0f, colony, false);
            assigner.UnregisterJob("recycler");
            Assert.That(released, Is.Zero);
            assigner.UnregisterJob("water");
            Assert.That(released, Is.EqualTo(1));
            assigner.Tick(0f, colony, false);
            Assert.That(colony.Cats[0].State, Is.EqualTo(CatState.Free));
        }

        [Test]
        public void PublicMethods_RejectInvalidInputs()
        {
            var assigner = Assigner();
            Assert.Throws<ArgumentNullException>(() => new JobAssigner(null));
            Assert.Throws<ArgumentNullException>(() => assigner.RegisterJob(null));
            Assert.Throws<KeyNotFoundException>(() => assigner.RegisterJob(Post("unknown")));
            assigner.RegisterJob(Post("water"));
            Assert.Throws<ArgumentException>(() => assigner.RegisterJob(Post("water")));
            Assert.Throws<KeyNotFoundException>(() => assigner.UnregisterJob("unknown"));
            Assert.Throws<KeyNotFoundException>(() => assigner.Release("unknown"));
            Assert.Throws<ArgumentException>(() => assigner.Release(" "));
            Assert.Throws<ArgumentException>(() => assigner.UnregisterJob(null));
            Assert.Throws<ArgumentNullException>(() => assigner.Tick(0f, null, false));
            foreach (float dt in new[] { -1f, float.NaN, float.PositiveInfinity })
                Assert.Throws<ArgumentOutOfRangeException>(() => assigner.Tick(dt, Colony(0), false));
        }
    }
}
