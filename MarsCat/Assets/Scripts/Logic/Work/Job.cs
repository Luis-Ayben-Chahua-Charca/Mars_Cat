using System;

namespace MarsCat.Logic.Work
{
    public sealed class Job
    {
        public string Id { get; }
        public JobKind Kind { get; }
        public string BuildingId { get; }
        public bool Outdoor { get; }
        public TaskPriority Priority { get; }

        public Job(string id, JobKind kind, string buildingId, bool outdoor,
            TaskPriority priority = TaskPriority.Normal)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El trabajo necesita un identificador.", nameof(id));
            if (string.IsNullOrWhiteSpace(buildingId))
                throw new ArgumentException("El trabajo necesita un identificador de edificio.", nameof(buildingId));
            if (!Enum.IsDefined(typeof(JobKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind));
            if (!Enum.IsDefined(typeof(TaskPriority), priority))
                throw new ArgumentOutOfRangeException(nameof(priority));
            Id = id;
            Kind = kind;
            BuildingId = buildingId;
            Outdoor = outdoor;
            Priority = priority;
        }
    }
}
