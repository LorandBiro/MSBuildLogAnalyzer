namespace MSBuildLogAnalyzer.Build
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public sealed class BuildRunner : IDisposable
    {
        private readonly Process process;

        public BuildRunner(string msBuildPath, string solutionPath, string logPath)
        {
            if (msBuildPath == null)
            {
                throw new ArgumentNullException(nameof(msBuildPath));
            }

            if (solutionPath == null)
            {
                throw new ArgumentNullException(nameof(solutionPath));
            }

            if (logPath == null)
            {
                throw new ArgumentNullException(nameof(logPath));
            }

            this.process = new Process
                {
                    StartInfo =
                        {
                            FileName = msBuildPath,
                            Arguments = $"\"{solutionPath}\" /target:Rebuild /verbosity:minimal /maxcpucount /fileLogger /fileLoggerParameters:LogFile=\"{logPath}\";Verbosity=diagnostic",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        },
                    EnableRaisingEvents = true
                };
        }

        public event DataReceivedEventHandler OutputReceived
        {
            add
            {
                this.process.OutputDataReceived += value;
                this.process.ErrorDataReceived += value;
            }

            remove
            {
                this.process.OutputDataReceived -= value;
                this.process.ErrorDataReceived -= value;
            }
        }

        public Task Run()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            this.process.Exited += (sender, e) => tcs.TrySetResult(true);
            this.process.Disposed += (sender, e) => tcs.TrySetCanceled();

            this.process.Start();
            this.process.BeginOutputReadLine();
            this.process.BeginErrorReadLine();

            return tcs.Task;
        }

        public void Dispose()
        {
            this.process.Dispose();
        }
    }
}
