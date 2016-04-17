namespace MSBuildLogAnalyzer.Model
{
    using System;
    using System.Collections.Generic;
    using MSBuildLogAnalyzer.Build;

    public sealed class ProjectTimeline
    {
        public ProjectBuild ProjectBuild { get; set; }

        public string Name { get; set; }

        public IReadOnlyList<RealWorkSegment> RealWork { get; set; }

        public TimeSpan StartedAt { get; set; }

        public TimeSpan CompletedAt { get; set; }

        public string Duration { get; set; }

        public TimeSpan RootStartedAt { get; set; }

        public TimeSpan RootCompletedAt { get; set; }

        public override string ToString()
        {
            // Needed for the ListBox so the user can jump to items typing the name.
            return this.Name;
        }
    }
}
