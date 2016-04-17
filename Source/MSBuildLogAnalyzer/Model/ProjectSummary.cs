namespace MSBuildLogAnalyzer.Model
{
    using System;
    using MSBuildLogAnalyzer.Build;

    public sealed class ProjectSummary
    {
        public ProjectBuild ProjectBuild { get; set; }

        public string Name { get; set; }

        public string Duration { get; set; }

        public double DurationRatio { get; set; }

        public override string ToString()
        {
            // Needed for the ListBox so the user can jump to items typing the name.
            return this.Name;
        }
    }
}
