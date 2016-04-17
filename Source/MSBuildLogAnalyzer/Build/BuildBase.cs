namespace MSBuildLogAnalyzer.Build
{
    using System;

    public abstract class BuildBase
    {
        protected BuildBase(string name, TimeSpan startedAt, TimeSpan completedAt)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            this.Name = name;
            this.StartedAt = startedAt;
            this.CompletedAt = completedAt;
        }

        public abstract BuildKind Kind { get; }

        public string Name { get; }

        public abstract string ShortName { get; }

        public TimeSpan StartedAt { get; }

        public TimeSpan CompletedAt { get; }

        public TimeSpan Duration => this.CompletedAt - this.StartedAt;

        public override string ToString()
        {
            return $"Kind: {this.Kind}, Name: {this.Name}, Duration: {this.Duration}";
        }
    }
}