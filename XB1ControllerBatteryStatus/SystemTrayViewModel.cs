using System.Linq;
using System.Threading;
using SharpDX.XInput;
using Windows.UI.Notifications;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Media;
using XB1ControllerBatteryStatus.ShellHelpers;
using MS.WindowsAPICodePack.Internal;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using XB1ControllerBatteryStatus.Localization;
using XB1ControllerBatteryStatus.Properties;

namespace XB1ControllerBatteryStatus
{
    public class SystemTrayViewModel : Caliburn.Micro.Screen
    {
        private string _activeIcon;
        private Controller _controller;
        private string _tooltipText;
        private const string APP_ID = "Simzy.XB1ControllerBatteryStatus";
        private bool[] toast_shown = new bool[5];
        private Dictionary<string, int> numdict = new Dictionary<string, int>();

        private SoundPlayer _soundPlayer;

        public SystemTrayViewModel()
        {

            ActiveIcon = "Resources/battery_unknown.ico";
            numdict["One"] = 1;
            numdict["Two"] = 2;
            numdict["Three"] = 3;
            numdict["Four"] = 4;
            TryCreateShortcut();
            Thread th = new Thread(RefreshControllerState);
            th.IsBackground = true;
            th.Start();
        }

        public string ActiveIcon
        {
            get { return _activeIcon; }
            set { Set(ref _activeIcon, value); }
        }

        public string TooltipText
        {
            get { return _tooltipText; }
            set { Set(ref _tooltipText, value); }
        }

        public ObservableCollection<CultureInfo> AvailableLanguages { get; } = new ObservableCollection<CultureInfo>();

        private void RefreshControllerState()
        {
            bool lowBatteryWarningSoundPlayed = false;

            while(true)
            {
                //Initialize controllers
                var controllers = new[]
                {
                    new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three),
                    new Controller(UserIndex.Four)
                };
                //Check if at least one is present
                _controller = controllers.FirstOrDefault(selectControler => selectControler.IsConnected);

                if (_controller != null)
                {
                    //cycle through all recognized controllers
                    foreach (var currentController in controllers)
                    {
                        var controllerIndexCaption = GetControllerIndexCaption(currentController.UserIndex);
                        if (currentController.IsConnected)
                        {
                            var batteryInfo = currentController.GetBatteryInformation(BatteryDeviceType.Gamepad);
                            //check if toast was already triggered and battery is no longer empty...
                            if (batteryInfo.BatteryLevel != BatteryLevel.Empty)
                            {
                                if (toast_shown[numdict[$"{currentController.UserIndex}"]] == true)
                                {
                                    //...reset the notification
                                    toast_shown[numdict[$"{currentController.UserIndex}"]] = false;
                                    ToastNotificationManager.History.Remove($"Controller{currentController.UserIndex}", "ControllerToast", APP_ID);
                                }
                            }
                            //wired
                            if (batteryInfo.BatteryType == BatteryType.Wired)
                            {
                                TooltipText = string.Format(Strings.ToolTip_Wired, controllerIndexCaption);
                                ActiveIcon = $"Resources/battery_wired_{currentController.UserIndex.ToString().ToLower()}.ico";
                            }
                            //"disconnected", a controller that was detected but hasn't sent battery data yet has this state
                            else if (batteryInfo.BatteryType == BatteryType.Disconnected)
                            {
                                TooltipText = string.Format(Strings.ToolTip_WaitingForData, controllerIndexCaption);
                                ActiveIcon = $"Resources/battery_disconnected_{currentController.UserIndex.ToString().ToLower()}.ico";
                            }
                            //this state should never happen
                            else if (batteryInfo.BatteryType == BatteryType.Unknown)
                            {
                                TooltipText = string.Format(Strings.ToolTip_Unknown, controllerIndexCaption);
                                ActiveIcon = $"Resources/battery_disconnected_{currentController.UserIndex.ToString().ToLower()}.ico";
                            }
                            //a battery level was detected
                            else
                            {
                                var batteryLevelCaption = GetBatteryLevelCaption(batteryInfo.BatteryLevel);
                                TooltipText = string.Format(Strings.ToolTip_Wireless, controllerIndexCaption, batteryLevelCaption);
                                ActiveIcon = $"Resources/battery_{batteryInfo.BatteryLevel.ToString().ToLower()}_{currentController.UserIndex.ToString().ToLower()}.ico";
                                //when "empty" state is detected...
                                if (batteryInfo.BatteryLevel == BatteryLevel.Empty)
                                {
                                   
                                    //check if notification sound is enabled
                                    if (Settings.Default.LowBatteryWarningSound_Enabled)
                                    {
                                        if (Settings.Default.LowBatteryWarningSound_Loop_Enabled || !lowBatteryWarningSoundPlayed)
                                        {
                                            //Necessary to avoid crashing if the .wav file is missing
                                            try
                                            {
                                                _soundPlayer?.Play();
                                            }
                                            catch (Exception ex)
                                            {
                                                Debug.WriteLine(ex);
                                            }
                                            lowBatteryWarningSoundPlayed = true;
                                        }
                                    }
                                }
                            }
                            Thread.Sleep(500);
                        }
                    }
                }
                else
                {
                    TooltipText = Strings.ToolTip_NoController;
                    ActiveIcon = "Resources/battery_unknown.ico";
                }
                Thread.Sleep(500);
            }
        }

        //try to create a start menu shortcut (required for sending toasts)
        private bool TryCreateShortcut()
        {
            String shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\XB1ControllerBatteryStatus.lnk";
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(shortcutPath);
                return true;
            }
            return false;
        }
        //create the shortcut
        private void InstallShortcut(String shortcutPath)
        {
            // Find the path to the current executable 
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe 
            ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

            // Open the shortcut property store, set the AppUserModelId property 
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(APP_ID))
            {
                ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk 
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }

        public void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }

        private string GetBatteryLevelCaption(BatteryLevel batteryLevel)
        {
            switch (batteryLevel)
            {
                case BatteryLevel.Empty:
                    return Strings.BatteryLevel_Empty;
                case BatteryLevel.Low:
                    return Strings.BatteryLevel_Low;
                case BatteryLevel.Medium:
                    return Strings.BatteryLevel_Medium;
                case BatteryLevel.Full:
                    return Strings.BatteryLevel_Full;
                default:
                    throw new ArgumentOutOfRangeException(nameof(batteryLevel), batteryLevel, null);
            }
        }

        private string GetControllerIndexCaption(UserIndex index)
        {
            switch (index)
            {
                case UserIndex.One:
                    return Strings.ControllerIndex_One;
                case UserIndex.Two:
                    return Strings.ControllerIndex_Two;
                case UserIndex.Three:
                    return Strings.ControllerIndex_Three;
                case UserIndex.Four:
                    return Strings.ControllerIndex_Four;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }
        }

        private void GetAvailableLanguages()
        {
            AvailableLanguages.Clear();
            foreach (var language in TranslationManager.AvailableLanguages)
            {
                AvailableLanguages.Add(language);
            }
        }

        public void UpdateNotificationSound()
        {
            _soundPlayer = File.Exists(Settings.Default.wavFile) ? new SoundPlayer(Settings.Default.wavFile) : null;
        }
    }
}