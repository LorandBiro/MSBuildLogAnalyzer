namespace MSBuildLogAnalyzer.Build
{
    using System;
    using System.Linq;

    public sealed class TargetBuild : BuildBase
    {
        private static readonly string[] NotRealWorkTargets = { "ResolveProjectReferences", "GetCopyToOutputDirectoryItems", "CleanReferencedProjects", "Rebuild" };

        public TargetBuild(string name, TimeSpan startedAt, TimeSpan completedAt, bool isSkipped)
            : base(name, startedAt, completedAt)
        {
            this.IsSkipped = isSkipped;
            this.RealWork = !NotRealWorkTargets.Contains(name);
        }

        public override BuildKind Kind => BuildKind.Target;

        public override string ShortName => this.IsSkipped ? $"{this.Name} (Skipped)" : $"{this.Name}";

        public bool IsSkipped { get; }

        public bool RealWork { get; }
    }
}