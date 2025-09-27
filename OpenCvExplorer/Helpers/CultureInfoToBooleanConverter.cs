using System.Globalization;
using System.Windows.Data;

namespace OpenCvExplorer.Helpers
{
    internal class CultureInfoToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not CultureInfo cultureInfo)
            {
                throw new ArgumentException("Exception CultureInfoToBooleanConverter value must be a CultureInfo");
            }

            bool isMatch = false;
            switch (parameter.ToString())
            {
                case "zh-Hans":
                    isMatch = cultureInfo.Name.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase);
                    break;
                case "en-US":
                    isMatch = cultureInfo.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase);
                    break;
                default:
                    throw new ArgumentException("Exception CultureInfoToBooleanConverter parameter must be a CultureInfo");
            }

            return isMatch;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CultureInfo cultureInfo)
            {
                throw new ArgumentException("Exception CultureInfoToBooleanConverter parameter must be a CultureInfo");
            }

            return cultureInfo.Name;
        }
    }
}
