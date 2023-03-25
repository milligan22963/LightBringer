using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LightBringer
{
    public class RoundingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                double dblValue = (double)value;
                return (int)dblValue;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double returnValue = 0.0;

            if (value != null)
            {
                string textValue = value as string;

                if (textValue != null)
                {
                    if (textValue.Length > 0)
                    {
                        returnValue = System.Convert.ToDouble(textValue);
                    }
                }
            }
            return returnValue;
        }
    }
}
