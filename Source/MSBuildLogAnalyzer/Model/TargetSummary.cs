namespace MSBuildLogAnalyzer.Model
{
    using System;

    public sealed class TargetSummary
    {
        public string Name { get; set; }

        public string Duration { get; set; }

        public string Count { get; set; }

        public double DurationRatio { get; set; }

        public double Opacity { get; set; }

        public override string ToString()
        {
            // Needed for the ListBox so the user can jump to items typing the name.
            return this.Name;
        }
    }
}
