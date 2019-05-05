using System;
using System.Globalization;
using System.Windows.Data;

namespace ExtendNetease_DGJModule
{
    //Refer:http://www.cnblogs.com/gaoshang212/p/4973300.html
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? false : value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
