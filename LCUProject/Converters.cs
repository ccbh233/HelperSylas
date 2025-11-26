using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HelperSylas
{
    public class IndexToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int idx && int.TryParse(parameter?.ToString(), out int target))
                return idx == target ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = value is bool b && b;
            if (parameter?.ToString() == "Inverse") visible = !visible;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class RankToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tier = value?.ToString()?.ToUpper() ?? "";
            string hex = tier switch
            {
                "CHALLENGER" => "#F4C874",
                "GRANDMASTER" => "#CD4545",
                "MASTER" => "#9D48E0",
                "DIAMOND" => "#576BCE",
                "EMERALD" => "#00A968",
                "PLATINUM" => "#4E9996",
                "GOLD" => "#CD853F",
                "SILVER" => "#80989D",
                "BRONZE" => "#8C5132",
                _ => "#8C8C8C"
            };
            try { return new BrushConverter().ConvertFrom(hex)!; } catch { return Brushes.Gray; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 保留此空类防止旧代码引用报错
    public class PercentToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 0;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}