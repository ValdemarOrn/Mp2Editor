using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Mp2Editor
{
    class BoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var val = (double)value;
                return val == 1.0 ? true : false;
            }
            else
            {
                var val = (bool)value;
                return val ? 1.0 : 0.0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var val = (double)value;
                return val == 1.0 ? true : false;
            }
            else
            {
                var val = (bool)value;
                return val ? 1.0 : 0.0;
            }
        }
    }
}
