using System;
using System.Globalization;
using System.Windows.Data;

namespace XB1ControllerBatteryIndicator
{
    public class IsCurrentLanguageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, TranslationManager.CurrentLanguage);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
