namespace MSBuildLogAnalyzer.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using MSBuildLogAnalyzer.Model;

    public sealed class ProjectTimelineToPathDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ProjectTimeline pt = (ProjectTimeline)value;
            if ((string)parameter == "full")
            {
                return PathGeometryHelper.CreateRectangle(pt.RootStartedAt, pt.RootCompletedAt, pt.StartedAt, pt.CompletedAt);
            }

            return PathGeometryHelper.CreateRectangles(pt.RootStartedAt, pt.RootCompletedAt, pt.RealWork);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
