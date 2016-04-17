namespace MSBuildLogAnalyzer.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public sealed class DurationRatioToPathDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return PathGeometryHelper.CreateRectangle(0.0, 1.0, 0.0, (double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
