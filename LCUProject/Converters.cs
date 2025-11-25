using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HelperSylas
{
    // 1. 索引转显隐 (页面切换)
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

    // 2. 布尔转显隐
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

    // 3. [修复] 补全 PercentToAngleConverter
    // 虽然我们为了防止崩溃暂时移除了动态圆环，但保留这个类防止 XAML 引用报错
    public class PercentToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                double angle = (percent / 100.0) * 360.0;
                return angle >= 360 ? 359.99 : angle;
            }
            return 0.0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 4. [修复] 补全 RankToColorConverter
    // 用于根据段位显示不同颜色的文字
    public class RankToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tier = value?.ToString()?.ToUpper() ?? "";
            string hex = tier switch
            {
                "CHALLENGER" => "#F4C874", // 王者金
                "GRANDMASTER" => "#CD4545", // 宗师红
                "MASTER" => "#9D48E0", // 大师紫
                "DIAMOND" => "#576BCE", // 钻石蓝
                "EMERALD" => "#00A968", // 翡翠绿
                "PLATINUM" => "#4E9996", // 铂金青
                "GOLD" => "#CD853F", // 黄金
                "SILVER" => "#80989D", // 白银
                "BRONZE" => "#8C5132", // 黄铜
                "IRON" => "#515151",   // 黑铁
                _ => "#8C8C8C"         // 默认灰
            };
            try
            {
                return new BrushConverter().ConvertFrom(hex)!;
            }
            catch
            {
                return Brushes.Gray;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}