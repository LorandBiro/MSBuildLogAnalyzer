namespace MSBuildLogAnalyzer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using Microsoft.Win32;
    using MSBuildLogAnalyzer.Build;
    using MSBuildLogAnalyzer.Properties;

    public partial class BuildWindow
    {
        public BuildWindow()
        {
            this.InitializeComponent();
            this.MsBuildPathTextBox.Text = Settings.Default.MSBuildPath;
            this.SolutionPathTextBox.Text = Settings.Default.LastSolutionPath;
        }

        private static string GetLogPath(string solutionPath)
        {
            const string LogFolderPath = "Logs";
            if (!Directory.Exists(LogFolderPath))
            {
                Directory.CreateDirectory(LogFolderPath);
            }

            DateTime now = DateTime.Now;
            string solutionName = Path.GetFileName(solutionPath);
            string logFileName = $"{now:yyyyMMdd}_{now:HHmmss}_{solutionName}.txt";

            return Path.Combine(Environment.CurrentDirectory, LogFolderPath, logFileName);
        }

        private void MsBuildBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { CheckFileExists = true, CheckPathExists = true, Filter = "Executable files (*.exe)|*.exe" };
            if (ofd.ShowDialog(this) == true)
            {
                this.MsBuildPathTextBox.Text = ofd.FileName;
            }
        }

        private void SolutionPathButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { CheckFileExists = true, CheckPathExists = true, Filter = "Solution and project files (*.sln, *.csproj)|*.sln;*.csproj" };
            if (ofd.ShowDialog(this) == true)
            {
                this.SolutionPathTextBox.Text = ofd.FileName;
            }
        }

        private async void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            this.OutputTextBox.Text = string.Empty;

            try
            {
                string msBuildPath = this.MsBuildPathTextBox.Text;
                string solutionPath = this.SolutionPathTextBox.Text;
                string logPath = GetLogPath(solutionPath);

                Stopwatch stopwatch = Stopwatch.StartNew();
                using (BuildRunner buildRunner = new BuildRunner(msBuildPath, solutionPath, logPath))
                {
                    buildRunner.OutputReceived += (sender2, e2) => this.Dispatcher.Invoke(
                        () =>
                            {
                                this.OutputTextBox.Text += e2.Data + Environment.NewLine;
                                this.OutputTextBox.ScrollToEnd();
                            });
                    await buildRunner.Run();
                }

                MessageBox.Show(this, $"Build took {stopwatch.Elapsed:g}\r\nLogs saved to {logPath}", "Build completed", MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.LastSolutionPath = solutionPath;
                Settings.Default.MSBuildPath = msBuildPath;
                Settings.Default.Save();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.ToString(), "Oops!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.IsEnabled = true;
        }
    }
}
