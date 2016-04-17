namespace MSBuildLogAnalyzer.Build
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ProjectBuildExtensions
    {
        public static IEnumerable<KeyValuePair<ProjectBuild, ProjectBuild[]>> GetAllMergedRealProjectBuilds(this ProjectBuild rootProjectBuild)
        {
            return
                rootProjectBuild.GetAllProjectBuilds()
                    .Where(projectBuild => projectBuild.HasAnyRealWorkTarget)
                    .GroupBy(projectBuild => projectBuild.Name)
                    .Where(projectBuildGroup => projectBuildGroup.Any())
                    .Select(projectBuildGroup => new KeyValuePair<ProjectBuild, ProjectBuild[]>(projectBuildGroup.Merge(), projectBuildGroup.ToArray()));
        }

        public static IEnumerable<TargetBuild> GetAllTargetBuilds(this ProjectBuild parentProjectBuild)
        {
            foreach (TargetBuild targetBuild in parentProjectBuild.ChildBuilds.OfType<TargetBuild>())
            {
                yield return targetBuild;
            }

            foreach (ProjectBuild projectBuild in parentProjectBuild.ChildBuilds.OfType<ProjectBuild>())
            {
                foreach (TargetBuild childTargetBuild in projectBuild.GetAllTargetBuilds())
                {
                    yield return childTargetBuild;
                }
            }
        }

        public static ProjectBuild[] GetPathTo(this ProjectBuild rootProjectBuild, ProjectBuild targetProjectBuild)
        {
            if (rootProjectBuild == targetProjectBuild)
            {
                return new[] { targetProjectBuild };
            }

            foreach (ProjectBuild projectBuild in rootProjectBuild.ChildBuilds.OfType<ProjectBuild>())
            {
                if (projectBuild == targetProjectBuild)
                {
                    return new[] { rootProjectBuild, targetProjectBuild };
                }

                ProjectBuild[] subPath = projectBuild.GetPathTo(targetProjectBuild);
                if (subPath != null)
                {
                    ProjectBuild[] path = new ProjectBuild[subPath.Length + 1];
                    path[0] = rootProjectBuild;
                    Array.Copy(subPath, 0, path, 1, subPath.Length);

                    return path;
                }
            }

            return null;
        }

        private static IEnumerable<ProjectBuild> GetAllProjectBuilds(this ProjectBuild parentProjectBuild)
        {
            yield return parentProjectBuild;

            foreach (ProjectBuild projectBuild in parentProjectBuild.ChildBuilds.OfType<ProjectBuild>())
            {
                foreach (ProjectBuild childProjectBuild in projectBuild.GetAllProjectBuilds())
                {
                    yield return childProjectBuild;
                }
            }
        }

        private static ProjectBuild Merge(this IEnumerable<ProjectBuild> projectBuilds)
        {
            ProjectBuild i = null;
            foreach (ProjectBuild projectBuild in projectBuilds)
            {
                i = i == null ? projectBuild : i.MergeWith(projectBuild);
            }

            if (i == null)
            {
                throw new InvalidOperationException("Can't merge an empty list of projects.");
            }

            return i;
        }
    }
}
