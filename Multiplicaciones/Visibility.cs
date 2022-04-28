using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Multiplicaciones
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool visibility = false;
            if (value is bool)
            {
                visibility = (bool)value;
            }
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return (visibility == Visibility.Visible);
        }
    }
}
