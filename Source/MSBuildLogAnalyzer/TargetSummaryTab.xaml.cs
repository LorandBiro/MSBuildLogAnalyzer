namespace MSBuildLogAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using MSBuildLogAnalyzer.Build;
    using MSBuildLogAnalyzer.Model;

    public partial class TargetSummaryTab
    {
        public TargetSummaryTab()
        {
            this.InitializeComponent();
        }

        public void SetRootProjectBuild(ProjectBuild rootProjectBuild)
        {
            var targetBuildGroups =
                rootProjectBuild.GetAllTargetBuilds()
                    .GroupBy(targetBuild => targetBuild.Name)
                    .Select(
                        targetBuildGroup =>
                        new
                            {
                                Name = targetBuildGroup.Key,
                                Duration = new TimeSpan(targetBuildGroup.Sum(targetBuild => targetBuild.Duration.Ticks)),
                                Count = targetBuildGroup.Count(),
                                RealWork = targetBuildGroup.Any(x => x.RealWork)
                            })
                    .ToList();

            TimeSpan maxDuration = targetBuildGroups.Max(targetBuildGroup => targetBuildGroup.Duration);
            IEnumerable<TargetSummary> targetSummaries = targetBuildGroups.Select(
                targetBuildGroup => new TargetSummary
                    {
                        Name = Path.GetFileName(targetBuildGroup.Name),
                        Duration = $"{targetBuildGroup.Duration.TotalSeconds:0.00} s",
                        Count = $"{targetBuildGroup.Count}x",
                        DurationRatio = targetBuildGroup.Duration.TotalSeconds / maxDuration.TotalSeconds,
                        Opacity = targetBuildGroup.RealWork ? 1.0 : 0.5
                    });

            this.TargetSummaryListBox.Items.Clear();
            foreach (TargetSummary ts in targetSummaries.OrderByDescending(x => x.DurationRatio))
            {
                this.TargetSummaryListBox.Items.Add(ts);
            }
        }
    }
}
