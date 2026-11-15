using System;
using System.Collections.Generic;
using System.Linq;
using MarsCat.Logic.Cats;

namespace MarsCat.Logic.Work
{
    public sealed class JobAssigner
    {
        private readonly WorkConfig _config;
        private readonly List<Job> _jobs = new List<Job>();
        private readonly Dictionary<string, Cat> _workers = new Dictionary<string, Cat>(StringComparer.Ordinal);

        public event Action<Job, Cat> OnAssigned;
        public event Action<Job, Cat> OnReleased;

        public JobAssigner(WorkConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void RegisterJob(Job job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            if (_jobs.Any(existing => existing.Id == job.Id))
                throw new ArgumentException("Trabajo repetido: " + job.Id, nameof(job));
            if (job.Kind == JobKind.Post)
                _config.Importance(job.BuildingId);
            _jobs.Add(job);
        }

        public void UnregisterJob(string id)
        {
            ValidateId(id, nameof(id));
            Job job = _jobs.FirstOrDefault(existing => existing.Id == id);
            if (job == null)
                throw new KeyNotFoundException("Trabajo desconocido: " + id);
            _jobs.Remove(job);
            ReleaseWorker(job);
        }

        public void Release(string buildingId)
        {
            ValidateId(buildingId, nameof(buildingId));
            Job[] jobs = _jobs.Where(job => job.BuildingId == buildingId).ToArray();
            if (jobs.Length == 0)
                throw new KeyNotFoundException("Edificio sin trabajos registrados: " + buildingId);
            foreach (Job job in jobs)
                ReleaseWorker(job);
        }

        public void Tick(float dt, CatColony colony, bool isNight)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
                throw new ArgumentOutOfRangeException(nameof(dt), "El tiempo debe ser finito y no negativo.");
            if (colony == null)
                throw new ArgumentNullException(nameof(colony));

            // Las instantáneas permiten que los eventos retiren trabajos durante el reparto.
            foreach (Job job in _jobs.ToArray())
                if (_workers.TryGetValue(job.Id, out Cat cat)
                    && (isNight || !colony.Cats.Contains(cat) || !IsWorking(cat)))
                    ReleaseWorker(job);
            if (isNight)
                return;

            Job[] pending = _jobs.Where(job => job.Kind == JobKind.OneOff)
                .OrderByDescending(job => job.Priority).Concat(_jobs.Where(job => job.Kind == JobKind.Post)).ToArray();
            foreach (Job job in pending)
            {
                if (!_jobs.Contains(job) || _workers.ContainsKey(job.Id))
                    continue;
                Cat cat = colony.FreeCats.Where(candidate => !job.Outdoor || !candidate.IsDegraded)
                    .OrderBy(candidate => candidate.Id).FirstOrDefault();
                if (cat == null && job.Kind == JobKind.OneOff)
                {
                    Job donor = _jobs.Where(candidate => candidate.Kind == JobKind.Post
                            && _workers.ContainsKey(candidate.Id)
                            && (!job.Outdoor || !_workers[candidate.Id].IsDegraded)
                            && (job.Priority == TaskPriority.Vital || _config.Importance(candidate.BuildingId) == 0))
                        .OrderBy(candidate => _config.Importance(candidate.BuildingId)).FirstOrDefault();
                    if (donor != null)
                    {
                        cat = _workers[donor.Id];
                        ReleaseWorker(donor);
                    }
                }
                if (cat == null || !_jobs.Contains(job) || _workers.ContainsKey(job.Id)
                    || !cat.TryAssign(job.Outdoor))
                    continue;
                _workers.Add(job.Id, cat);
                cat.StateChanged += HandleStateChanged;
                OnAssigned?.Invoke(job, cat);
            }
        }

        private void HandleStateChanged(Cat cat, CatState from, CatState to)
        {
            if (IsWorking(cat))
                return;
            Job job = _jobs.FirstOrDefault(candidate => _workers.TryGetValue(candidate.Id, out Cat worker)
                && ReferenceEquals(worker, cat));
            if (job != null)
                ReleaseWorker(job);
        }

        private void ReleaseWorker(Job job)
        {
            if (!_workers.TryGetValue(job.Id, out Cat cat))
                return;
            _workers.Remove(job.Id);
            cat.StateChanged -= HandleStateChanged;
            cat.Release();
            OnReleased?.Invoke(job, cat);
        }

        private static bool IsWorking(Cat cat)
        {
            return cat.State == CatState.WorkingIndoor || cat.State == CatState.WorkingOutdoor;
        }

        private static void ValidateId(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El identificador no puede estar vacío.", name);
        }
    }
}
