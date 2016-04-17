namespace MSBuildLogAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using MSBuildLogAnalyzer.Build;
    using MSBuildLogAnalyzer.Model;

    public partial class ProjectSummaryTab
    {
        public ProjectSummaryTab()
        {
            this.InitializeComponent();
        }

        public event EventHandler<StepIntoRequestedEventArgs> StepIntoRequested;

        public void SetRootProjectBuild(ProjectBuild rootProjectBuild)
        {
            List<KeyValuePair<ProjectBuild, ProjectBuild[]>> projectBuilds = rootProjectBuild.GetAllMergedRealProjectBuilds().ToList();

            TimeSpan maxDuration = projectBuilds.Max(projectBuild => projectBuild.Key.RealDuration);
            IEnumerable<ProjectSummary> buildTimelines =
                projectBuilds.Select(
                    projectBuild =>
                    new ProjectSummary
                        {
                            ProjectBuild = projectBuild.Key,
                            Name = Path.GetFileName(projectBuild.Key.Name),
                            Duration = $"{projectBuild.Key.RealDuration.TotalSeconds:0.00} s",
                            DurationRatio = projectBuild.Key.RealDuration.TotalSeconds / maxDuration.TotalSeconds,
                        });

            this.ProjectSummaryListBox.Items.Clear();
            foreach (ProjectSummary ps in buildTimelines.OrderByDescending(x => x.DurationRatio))
            {
                this.ProjectSummaryListBox.Items.Add(ps);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    this.StepInto();
                    break;
            }
        }

        private void StepIntoButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.StepInto();
        }

        private void BuildTimelineListBox_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null)
            {
                return;
            }

            ListBoxItem item = ItemsControl.ContainerFromElement(this.ProjectSummaryListBox, originalSource) as ListBoxItem;
            if (item == null)
            {
                return;
            }

            this.StepInto();
        }

        private void StepInto()
        {
            ProjectSummary ps = this.ProjectSummaryListBox.SelectedItem as ProjectSummary;
            if (ps == null)
            {
                return;
            }

            this.StepIntoRequested?.Invoke(this, new StepIntoRequestedEventArgs(ps.ProjectBuild));
        }

        public class StepIntoRequestedEventArgs : EventArgs
        {
            public StepIntoRequestedEventArgs(ProjectBuild projectBuild)
            {
                if (projectBuild == null)
                {
                    throw new ArgumentNullException(nameof(projectBuild));
                }

                this.ProjectBuild = projectBuild;
            }

            public ProjectBuild ProjectBuild { get; }
        }
    }
}
