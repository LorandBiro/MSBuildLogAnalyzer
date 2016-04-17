namespace MSBuildLogAnalyzer.Model
{
    using System;
    using System.Windows;
    using MSBuildLogAnalyzer.Build;

    public sealed class BuildTimeline
    {
        public BuildBase Build { get; set; }

        public string Name { get; set; }

        public FontWeight FontWeight { get; set; }
        
        public TimeSpan StartedAt { get; set; }

        public TimeSpan CompletedAt { get; set; }
        
        public string Duration { get; set; }

        public TimeSpan ParentStartedAt { get; set; }

        public TimeSpan ParentCompletedAt { get; set; }

        public override string ToString()
        {
            // Needed for the ListBox so the user can jump to items typing the name.
            return this.Name;
        }
    }
}
