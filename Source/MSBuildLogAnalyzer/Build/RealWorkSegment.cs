namespace MSBuildLogAnalyzer.Build
{
    using System;

    public sealed class RealWorkSegment
    {
        public RealWorkSegment(TimeSpan startedAt, TimeSpan completedAt)
        {
            this.StartedAt = startedAt;
            this.CompletedAt = completedAt;
        }

        public TimeSpan StartedAt { get; }

        public TimeSpan CompletedAt { get; }

        public override string ToString()
        {
            return $"{StartedAt} - {CompletedAt}";
        }
    }
}