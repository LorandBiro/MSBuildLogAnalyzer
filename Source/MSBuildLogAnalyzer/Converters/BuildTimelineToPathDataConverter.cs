namespace MSBuildLogAnalyzer.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using MSBuildLogAnalyzer.Model;

    public sealed class BuildTimelineToPathDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BuildTimeline bt = (BuildTimeline)value;
            return PathGeometryHelper.CreateRectangle(bt.ParentStartedAt, bt.ParentCompletedAt, bt.StartedAt, bt.CompletedAt);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
