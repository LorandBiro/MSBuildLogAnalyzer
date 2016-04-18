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

    public partial class ProjectTimelineTab
    {
        public ProjectTimelineTab()
        {
            this.InitializeComponent();
        }

        public event EventHandler<StepIntoRequestedEventArgs> StepIntoRequested;

        public void SetRootProjectBuild(ProjectBuild rootProjectBuild)
        {
            IEnumerable<ProjectTimeline> projectTimelines =
                rootProjectBuild.GetAllMergedRealProjectBuilds()
                    .Select(
                        projectBuild =>
                        new ProjectTimeline
                            {
                                ProjectBuild = projectBuild,
                                Name = projectBuild.ShortName,
                                RealWork = projectBuild.RealWork,
                                StartedAt = projectBuild.StartedAt,
                                CompletedAt = projectBuild.CompletedAt,
                                Duration = $"{projectBuild.RealDuration.TotalSeconds:0.00} s",
                                RootStartedAt = rootProjectBuild.StartedAt,
                                RootCompletedAt = rootProjectBuild.CompletedAt
                            });

            this.ProjectTimelineListBox.Items.Clear();
            foreach (ProjectTimeline pt in projectTimelines.OrderBy(x => x.CompletedAt))
            {
                this.ProjectTimelineListBox.Items.Add(pt);
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

        private void ProjectTimelineListBox_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null)
            {
                return;
            }

            ListBoxItem item = ItemsControl.ContainerFromElement(this.ProjectTimelineListBox, originalSource) as ListBoxItem;
            if (item == null)
            {
                return;
            }

            this.StepInto();
        }

        private void StepInto()
        {
            ProjectTimeline pt = this.ProjectTimelineListBox.SelectedItem as ProjectTimeline;
            if (pt == null)
            {
                return;
            }

            this.StepIntoRequested?.Invoke(this, new StepIntoRequestedEventArgs(pt.ProjectBuild));
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
