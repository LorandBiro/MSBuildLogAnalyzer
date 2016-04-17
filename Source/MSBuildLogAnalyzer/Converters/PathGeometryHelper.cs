namespace MSBuildLogAnalyzer.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using MSBuildLogAnalyzer.Build;

    public static class PathGeometryHelper
    {
        public static PathGeometry CreateRectangle(double rangeLeft, double rangeRight, double left, double right)
        {
            return new PathGeometry(new[] { CreatePointFigure(rangeLeft, 0.0), CreatePointFigure(rangeRight, 1.0), CreateRectangleFigure(left, right) });
        }

        public static PathGeometry CreateRectangle(TimeSpan rootProjectBuildStartedAt, TimeSpan rootProjectBuildCompletedAt, TimeSpan startedAt, TimeSpan completedAt)
        {
            double duration = (rootProjectBuildCompletedAt - rootProjectBuildStartedAt).TotalSeconds;
            double left = (startedAt - rootProjectBuildStartedAt).TotalSeconds / duration;
            double right = (completedAt - rootProjectBuildStartedAt).TotalSeconds / duration;

            return CreateRectangle(0.0, 1.0, left, right);
        }

        public static PathGeometry CreateRectangles(TimeSpan rootProjectBuildStartedAt, TimeSpan rootProjectBuildCompletedAt, IReadOnlyList<RealWorkSegment> realWork)
        {
            double duration = (rootProjectBuildCompletedAt - rootProjectBuildStartedAt).TotalSeconds;

            List<PathFigure> figures = new List<PathFigure> { CreatePointFigure(0.0, 0.0), CreatePointFigure(1.0, 1.0) };
            figures.AddRange(
                realWork.Select(
                    realWorkSegment =>
                    CreateRectangleFigure(
                        (realWorkSegment.StartedAt - rootProjectBuildStartedAt).TotalSeconds / duration,
                        (realWorkSegment.CompletedAt - rootProjectBuildStartedAt).TotalSeconds / duration)));
            
            return new PathGeometry(figures);
        }

        private static PathFigure CreatePointFigure(double left, double top)
        {
            return new PathFigure(new Point(left, top), new PathSegment[0], false);
        }

        private static PathFigure CreateRectangleFigure(double left, double right)
        {
            return new PathFigure(
                new Point(left, 0.0),
                new PathSegment[] { new PolyLineSegment(new[] { new Point(right, 0.0), new Point(right, 1.0), new Point(left, 1.0) }, true) },
                true);
        }
    }
}
