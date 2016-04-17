namespace MSBuildLogAnalyzer
{
    using System;
    using System.IO;
    using System.Windows;
    using Microsoft.Win32;
    using MSBuildLogAnalyzer.Build;

    public partial class MainWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void OpenLogFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "All files (*.*)|*.*",
                    InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Logs")
                };

            if (ofd.ShowDialog(this) == true)
            {
                ProjectBuild rootProjectBuild = BuildLogParser.GetProjectBuild(ofd.FileName);
                this.Title = $"MSBuild Log Analyzer - {Path.GetFileName(ofd.FileName)}";
                this.TimelineTab.SetRootProjectBuild(rootProjectBuild);
                this.ProjectSummaryTab.SetRootProjectBuild(rootProjectBuild);
                this.ProjectTimelineTab.SetRootProjectBuild(rootProjectBuild);
                this.TargetSummaryTab.SetRootProjectBuild(rootProjectBuild);
            }
        }

        private void RunBuildButton_OnClick(object sender, RoutedEventArgs e)
        {
            BuildWindow buildWindow = new BuildWindow { Owner = this };
            buildWindow.ShowDialog();
        }

        private void ProjectSummaryTab_OnStepIntoRequested(object sender, ProjectSummaryTab.StepIntoRequestedEventArgs e)
        {
            this.TabControl.SelectedIndex = 0;
            this.TimelineTab.StepInto(e.ProjectBuild);
        }

        private void ProjectTimelineTab_OnStepIntoRequested(object sender, ProjectTimelineTab.StepIntoRequestedEventArgs e)
        {
            this.TabControl.SelectedIndex = 0;
            this.TimelineTab.StepInto(e.ProjectBuild);
        }
    }
}
