namespace MSBuildLogAnalyzer.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class BuildLogParser
    {
        private static readonly Regex LogLineRegex = new Regex(@"^(?<timestamp>\d\d:\d\d:\d\d.\d\d\d)\s+(?<threadId>[\d:]+)\>(?<text>.+)$", RegexOptions.Compiled);

        private static readonly Regex TargetStartedRegex = new Regex(@"^Target ""(?<targetName>[^:""]*):", RegexOptions.Compiled);

        private static readonly Regex TargetDoneRegex = new Regex(@"^Done building target ""(?<targetName>[^""]*)"" in project", RegexOptions.Compiled);

        private static readonly Regex TargetSkippedRegex = new Regex(@"^Target ""(?<targetName>[^""]*)"" skipped", RegexOptions.Compiled);

        private static readonly Regex RootProjectStartedRegex = new Regex(@"^Project ""(?<projectName>.*?)"" on node \d+ \((?<projectTarget>.*?) target(s|\(s\))\)", RegexOptions.Compiled);

        private static readonly Regex ProjectStartedRegex =
            new Regex(@"^Project .*? is building ""(?<projectName>.*?)"" \((?<childThreadId>[\d:]+)\) on node \d+ \((?<projectTarget>.*?) target(s|\(s\))\)", RegexOptions.Compiled);

        private static readonly Regex ProjectDoneRegex = new Regex(@"^Done Building Project ""(?<projectName>.*?)""", RegexOptions.Compiled);

        private enum LogEventKind
        {
            RootProjectStarted,

            ProjectStarted,

            ProjectDone,

            TargetStarted,

            TargetDone,

            TargetSkipped,
        }

        public static ProjectBuild GetProjectBuild(string logFilePath)
        {
            List<LogEvent> logEvents = GetLogEvents(logFilePath).ToList();
            return GetProjectBuild(logEvents, null, null, "1");
        }

        private static ProjectBuild GetProjectBuild(IList<LogEvent> logEvents, string name, string targets, string threadId)
        {
            IList<LogEvent> projectLogEvents = logEvents.Where(x => x.ThreadId == threadId).ToList();

            if (name == null || targets == null)
            {
                if (projectLogEvents[0].Kind != LogEventKind.RootProjectStarted)
                {
                    throw new Exception();
                }

                name = projectLogEvents[0].ProjectOrTargetName;
                targets = projectLogEvents[0].ProjectTarget;
            }

            List<BuildBase> childBuilds = GetChildBuilds(logEvents, projectLogEvents).ToList();

            if (projectLogEvents[projectLogEvents.Count - 1].ProjectOrTargetName != name)
            {
                throw new Exception();
            }

            TimeSpan startedAt = projectLogEvents[0].Timestamp;
            TimeSpan completedAt = projectLogEvents[projectLogEvents.Count - 1].Timestamp;
            return new ProjectBuild(name, startedAt, completedAt, targets, threadId, childBuilds);
        }

        private static IEnumerable<BuildBase> GetChildBuilds(IList<LogEvent> logEvents, IList<LogEvent> projectLogEvents)
        {
            for (int i = 0; i < projectLogEvents.Count; i++)
            {
                switch (projectLogEvents[i].Kind)
                {
                    case LogEventKind.TargetStarted:
                        {
                            string name = projectLogEvents[i].ProjectOrTargetName;
                            TimeSpan startedAt = projectLogEvents[i].Timestamp;
                            bool found = false;
                            for (int j = i + 1; j < projectLogEvents.Count; j++)
                            {
                                if (projectLogEvents[j].Kind == LogEventKind.TargetDone && projectLogEvents[j].ProjectOrTargetName == name)
                                {
                                    TimeSpan childCompletedAt = projectLogEvents[j].Timestamp;
                                    yield return new TargetBuild(name, startedAt, childCompletedAt, false);
                                    found = true;
                                    break;
                                }
                            }

                            if (found)
                            {
                                break;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }

                    case LogEventKind.TargetSkipped:
                        {
                            string name = projectLogEvents[i].ProjectOrTargetName;
                            TimeSpan timestamp = projectLogEvents[i].Timestamp;

                            yield return new TargetBuild(name, timestamp, timestamp, true);
                            break;
                        }

                    case LogEventKind.ProjectStarted:
                        {
                            string name = projectLogEvents[i].ProjectOrTargetName;
                            string targets = projectLogEvents[i].ProjectTarget;
                            string threadId = projectLogEvents[i].ChildThreadId;

                            yield return GetProjectBuild(logEvents, name, targets, threadId);
                            break;
                        }
                }
            }
        }

        private static IEnumerable<LogEvent> GetLogEvents(string logFilePath)
        {
            foreach (LogLine logLine in GetLogLines(logFilePath))
            {
                Match rootProjectStartedMatch = RootProjectStartedRegex.Match(logLine.Text);
                if (rootProjectStartedMatch.Success)
                {
                    string projectName = rootProjectStartedMatch.Groups["projectName"].Value;
                    string projectTarget = rootProjectStartedMatch.Groups["projectTarget"].Value;

                    yield return new LogEvent(logLine.Timestamp, logLine.ThreadId, LogEventKind.RootProjectStarted, projectName, null, projectTarget);
                }

                Match projectStartedMatch = ProjectStartedRegex.Match(logLine.Text);
                if (projectStartedMatch.Success)
                {
                    string projectName = projectStartedMatch.Groups["projectName"].Value;
                    string childThreadId = projectStartedMatch.Groups["childThreadId"].Value;
                    string projectTarget = projectStartedMatch.Groups["projectTarget"].Value;

                    yield return new LogEvent(logLine.Timestamp, logLine.ThreadId, LogEventKind.ProjectStarted, projectName, childThreadId, projectTarget);
                }

                Match projectDoneMatch = ProjectDoneRegex.Match(logLine.Text);
                if (projectDoneMatch.Success)
                {
                    string projectName = projectDoneMatch.Groups["projectName"].Value;

                    yield return new LogEvent(logLine.Timestamp, logLine.ThreadId, LogEventKind.ProjectDone, projectName, null, null);
                }

                Match targetStartedMatch = TargetStartedRegex.Match(logLine.Text);
                if (targetStartedMatch.Success)
                {
                    string targetName = targetStartedMatch.Groups["targetName"].Value;

                    yield return new LogEvent(logLine.Timestamp, logLine.ThreadId, LogEventKind.TargetStarted, targetName, null, null);
                }

                Match targetDoneMatch = TargetDoneRegex.Match(logLine.Text);
                if (targetDoneMatch.Success)
                {
                    string targetName = targetDoneMatch.Groups["targetName"].Value;

                    yield return new LogEvent(logLine.Timestamp, logLine.ThreadId, LogEventKind.TargetDone, targetName, null, null);
                }

                Match targetSkippedMatch = TargetSkippedRegex.Match(logLine.Text);
                if (targetSkippedMatch.Success)
                {
                    string targetName = targetSkippedMatch.Groups["targetName"].Value;

                    yield return new LogEvent(logLine.Timestamp, logLine.ThreadId, LogEventKind.TargetSkipped, targetName, null, null);
                }
            }
        }

        private static IEnumerable<LogLine> GetLogLines(string logFilePath)
        {
            foreach (string line in File.ReadLines(logFilePath))
            {
                if (line.Length < 19 || !char.IsDigit(line[0]))
                {
                    continue;
                }

                Match match = LogLineRegex.Match(line);
                if (match.Success)
                {
                    TimeSpan timestamp = TimeSpan.Parse(match.Groups["timestamp"].Value);
                    string threadId = match.Groups["threadId"].Value;
                    string text = match.Groups["text"].Value;

                    yield return new LogLine(timestamp, threadId, text);
                }
            }
        }

        private class LogLine
        {
            public LogLine(TimeSpan timestamp, string threadId, string text)
            {
                if (string.IsNullOrEmpty(threadId))
                {
                    throw new ArgumentException("Value cannot be null or empty.", nameof(threadId));
                }

                if (string.IsNullOrEmpty(text))
                {
                    throw new ArgumentException("Value cannot be null or empty.", nameof(text));
                }

                this.Timestamp = timestamp;
                this.ThreadId = threadId;
                this.Text = text;
            }

            public TimeSpan Timestamp { get; }

            public string ThreadId { get; }

            public string Text { get; }

            public override string ToString()
            {
                return $"Timestamp: {this.Timestamp}, ThreadId: {this.ThreadId}, Text: {this.Text}";
            }
        }

        private class LogEvent
        {
            public LogEvent(TimeSpan timestamp, string threadId, LogEventKind kind, string projectOrTargetName, string childThreadId, string projectTarget)
            {
                if (string.IsNullOrEmpty(threadId))
                {
                    throw new ArgumentException("Value cannot be null or empty.", nameof(threadId));
                }

                if (string.IsNullOrEmpty(projectOrTargetName))
                {
                    throw new ArgumentException("Value cannot be null or empty.", nameof(projectOrTargetName));
                }

                this.Timestamp = timestamp;
                this.ThreadId = threadId;
                this.Kind = kind;
                this.ProjectOrTargetName = projectOrTargetName;
                this.ChildThreadId = childThreadId;
                this.ProjectTarget = projectTarget;
            }

            public TimeSpan Timestamp { get; }

            public string ThreadId { get; }

            public LogEventKind Kind { get; }

            public string ProjectOrTargetName { get; }

            public string ChildThreadId { get; }

            public string ProjectTarget { get; }

            public override string ToString()
            {
                return $"Timestamp: {this.Timestamp}, ThreadId: {this.ThreadId}, Kind: {this.Kind}, ProjectOrTargetName: {this.ProjectOrTargetName}, ChildThreadId: {this.ChildThreadId}, ProjectTarget: {this.ProjectTarget}";
            }
        }
    }
}
