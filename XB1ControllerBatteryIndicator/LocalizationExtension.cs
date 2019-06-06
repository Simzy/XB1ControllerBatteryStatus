using System.Windows.Data;

namespace XB1ControllerBatteryIndicator
{
    public class LocalizationExtension : Binding
    {
        public LocalizationExtension(string name) : base("[" + name + "]")
        {
            this.Mode = BindingMode.OneWay;
            this.Source = TranslationBindingProvider.Instance;
        }
    }
}