namespace MSBuildLogAnalyzer.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public sealed class ProjectBuild : BuildBase
    {
        private static readonly TimeSpan GapLimit = TimeSpan.FromMilliseconds(1.0);

        public ProjectBuild(string name, TimeSpan startedAt, TimeSpan completedAt, string targets, string threadId, IEnumerable<BuildBase> childBuilds)
            : base(name, startedAt, completedAt)
        {
            if (string.IsNullOrEmpty(targets))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(targets));
            }

            if (string.IsNullOrEmpty(threadId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(threadId));
            }

            if (childBuilds == null)
            {
                throw new ArgumentNullException(nameof(childBuilds));
            }

            this.Targets = targets;
            this.ThreadId = threadId;
            this.ChildBuilds = childBuilds.ToList();

            this.HasAnyRealWorkTarget = this.ChildBuilds.OfType<TargetBuild>().Any(x => x.RealWork);
            if (this.HasAnyRealWorkTarget)
            {
                this.RealWork = GetRealWork(this.ChildBuilds).ToList();
                this.RealDuration = new TimeSpan(this.RealWork.Sum(x => (x.CompletedAt - x.StartedAt).Ticks));
            }
            else
            {
                this.RealWork = new[] { new RealWorkSegment(this.StartedAt, this.CompletedAt) };
                this.RealDuration = this.Duration;
            }
        }

        public override BuildKind Kind => BuildKind.Project;

        public override string ShortName => $"{Path.GetFileName(this.Name)} ({this.Targets})";

        public string Targets { get; }

        public string ThreadId { get; }

        public IReadOnlyList<BuildBase> ChildBuilds { get; }

        public bool HasAnyRealWorkTarget { get; }
        
        public IReadOnlyList<RealWorkSegment> RealWork { get; }

        public TimeSpan RealDuration { get; }

        public ProjectBuild MergeWith(ProjectBuild otherProjectBuild)
        {
            if (otherProjectBuild == null)
            {
                throw new ArgumentNullException(nameof(otherProjectBuild));
            }

            if (otherProjectBuild.Name != this.Name)
            {
                throw new ArgumentException("Can't merge differently named projects.");
            }

            return new ProjectBuild(
                this.Name,
                this.StartedAt < otherProjectBuild.StartedAt ? this.StartedAt : otherProjectBuild.StartedAt,
                this.CompletedAt > otherProjectBuild.CompletedAt ? this.CompletedAt : otherProjectBuild.CompletedAt,
                $"{this.Targets}, {otherProjectBuild.Targets}",
                $"{this.ThreadId}, {otherProjectBuild.ThreadId}",
                this.ChildBuilds.Concat(otherProjectBuild.ChildBuilds).OrderBy(x => x.StartedAt));
        }

        private static IEnumerable<RealWorkSegment> GetRealWork(IEnumerable<BuildBase> childBuilds)
        {
            List<TargetBuild> targetBuilds = childBuilds.OfType<TargetBuild>().Where(x => x.RealWork).OrderBy(x => x.StartedAt).ToList();
            if (targetBuilds.Count == 0)
            {
                yield break;
            }

            TimeSpan? startedAt = null;
            TimeSpan? completedAt = null;
            foreach (TargetBuild targetBuild in targetBuilds)
            {
                if (startedAt == null)
                {
                    startedAt = targetBuild.StartedAt;
                    completedAt = targetBuild.CompletedAt;
                    continue;
                }

                if (targetBuild.StartedAt - completedAt.Value <= GapLimit)
                {
                    if (targetBuild.CompletedAt > completedAt)
                    {
                        completedAt = targetBuild.CompletedAt;
                    }

                    continue;
                }

                yield return new RealWorkSegment(startedAt.Value, completedAt.Value);
                startedAt = targetBuild.StartedAt;
                completedAt = targetBuild.CompletedAt;
            }
            
            if (startedAt != null)
            {
                yield return new RealWorkSegment(startedAt.Value, completedAt.Value);
            }
        }
    }
}