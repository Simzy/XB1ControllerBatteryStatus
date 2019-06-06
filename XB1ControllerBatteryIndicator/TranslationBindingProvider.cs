using Caliburn.Micro;
using XB1ControllerBatteryIndicator.Localization;

namespace XB1ControllerBatteryIndicator
{
    public class TranslationBindingProvider : PropertyChangedBase
    {
        public static TranslationBindingProvider Instance { get; } = new TranslationBindingProvider();

        private TranslationBindingProvider()
        {
            TranslationManager.CurrentLanguageChangedEvent += (sender, args) => NotifyOfPropertyChange(string.Empty);
        }

        public string this[string key] => Strings.ResourceManager.GetString(key);
    }
}
