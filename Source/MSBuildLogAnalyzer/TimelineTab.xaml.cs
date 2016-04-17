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

    public partial class TimelineTab
    {
        private readonly Stack<ProjectBuild> stack = new Stack<ProjectBuild>();

        private ProjectBuild rootProjectBuild;

        public TimelineTab()
        {
            this.InitializeComponent();
        }

        public void SetRootProjectBuild(ProjectBuild rootProjectBuild)
        {
            if (rootProjectBuild == null)
            {
                throw new ArgumentNullException(nameof(rootProjectBuild));
            }

            this.rootProjectBuild = rootProjectBuild;

            this.stack.Clear();
            this.stack.Push(this.rootProjectBuild);
            this.UpdateBreadcrumb();
            this.UpdateBuildTimelineListBox(this.rootProjectBuild);

            BuildTimeline firstBt = this.BuildTimelineListBox.Items.OfType<BuildTimeline>().FirstOrDefault();
            if (firstBt != null)
            {
                this.BuildTimelineListBox.SelectedItem = firstBt;
                this.BuildTimelineListBox.UpdateLayout();
                ListBoxItem listBoxItem = (ListBoxItem)this.BuildTimelineListBox.ItemContainerGenerator.ContainerFromItem(firstBt);
                listBoxItem?.Focus();
            }
        }

        public void StepInto(ProjectBuild projectBuild)
        {
            if (projectBuild == null)
            {
                throw new ArgumentNullException(nameof(projectBuild));
            }

            this.stack.Clear();
            ProjectBuild[] path = this.rootProjectBuild.GetPathTo(projectBuild) ?? new[] { this.rootProjectBuild, projectBuild };
            foreach (ProjectBuild p in path)
            {
                this.stack.Push(p);
            }

            this.UpdateBreadcrumb();
            this.UpdateBuildTimelineListBox(projectBuild);

            BuildTimeline firstBt = this.BuildTimelineListBox.Items.OfType<BuildTimeline>().FirstOrDefault();
            if (firstBt != null)
            {
                this.BuildTimelineListBox.SelectedItem = firstBt;
                this.BuildTimelineListBox.UpdateLayout();
                ListBoxItem listBoxItem = (ListBoxItem)this.BuildTimelineListBox.ItemContainerGenerator.ContainerFromItem(firstBt);
                listBoxItem.Focus();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    this.StepInto();
                    break;
                case Key.Back:
                    this.StepOut();
                    break;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton1)
            {
                this.StepOut();
            }
        }

        private void StepIntoButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.StepInto();
        }

        private void StepOutButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.StepOut();
        }

        private void BuildTimelineListBox_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null)
            {
                return;
            }

            ListBoxItem item = ItemsControl.ContainerFromElement(this.BuildTimelineListBox, originalSource) as ListBoxItem;
            if (item == null)
            {
                return;
            }

            this.StepInto();
        }

        private void StepInto()
        {
            BuildTimeline bt = this.BuildTimelineListBox.SelectedItem as BuildTimeline;
            if (bt == null || bt.Build.Kind != BuildKind.Project)
            {
                return;
            }

            ProjectBuild build = (ProjectBuild)bt.Build;

            this.stack.Push(build);
            this.UpdateBreadcrumb();
            this.UpdateBuildTimelineListBox(build);

            BuildTimeline firstBt = this.BuildTimelineListBox.Items.OfType<BuildTimeline>().FirstOrDefault();
            if (firstBt != null)
            {
                this.BuildTimelineListBox.SelectedItem = firstBt;
                this.BuildTimelineListBox.UpdateLayout();
                ListBoxItem listBoxItem = (ListBoxItem)this.BuildTimelineListBox.ItemContainerGenerator.ContainerFromItem(firstBt);
                listBoxItem.Focus();
            }
        }

        private void StepOut()
        {
            if (this.stack.Count <= 1)
            {
                return;
            }

            BuildBase previousBuild = this.stack.Pop();
            this.UpdateBreadcrumb();
            this.UpdateBuildTimelineListBox(this.stack.Peek());

            BuildTimeline previousBt = this.BuildTimelineListBox.Items.OfType<BuildTimeline>().SingleOrDefault(x => x.Build == previousBuild);
            if (previousBt == null)
            {
                previousBt = this.BuildTimelineListBox.Items.OfType<BuildTimeline>().FirstOrDefault();
            }

            this.BuildTimelineListBox.SelectedItem = previousBt;
            this.BuildTimelineListBox.UpdateLayout();
            ListBoxItem listBoxItem = (ListBoxItem)this.BuildTimelineListBox.ItemContainerGenerator.ContainerFromItem(previousBt);
            listBoxItem.Focus();
        }

        private void UpdateBreadcrumb()
        {
            this.BreadcrumbTextBlock.Text = string.Join(" > ", this.stack.Reverse().Select(x => $"{Path.GetFileName(x.Name)} ({x.Targets})"));
            this.DurationTextBlock.Text = $"{this.stack.Peek().Duration.TotalSeconds:0.00} s";
        }

        private void UpdateBuildTimelineListBox(ProjectBuild build)
        {
            this.BuildTimelineListBox.Items.Clear();
            IEnumerable<BuildTimeline> buildTimelines =
                build.ChildBuilds.Select(
                    childBuild =>
                    new BuildTimeline
                        {
                            Build = childBuild,
                            StartedAt = childBuild.StartedAt,
                            CompletedAt = childBuild.CompletedAt,
                            Duration = $"{(int)childBuild.Duration.TotalMilliseconds} ms",
                            Name = childBuild.ShortName,
                            FontWeight = childBuild.Kind == BuildKind.Project ? FontWeights.Bold : FontWeights.Normal,
                            ParentStartedAt = build.StartedAt,
                            ParentCompletedAt = build.CompletedAt
                        });

            foreach (BuildTimeline bt in buildTimelines.OrderBy(x => x.Build.StartedAt))
            {
                this.BuildTimelineListBox.Items.Add(bt);
            }
        }
    }
}
