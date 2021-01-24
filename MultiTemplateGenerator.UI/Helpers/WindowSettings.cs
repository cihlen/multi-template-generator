// ****************************************************************************
// <author>James Witt Hurst</author>
// <email>JamesH@DesignForge.com</email>
// <date>2011-8-15</date>
// <project>BaseLib</project>
// <web>http://www.designforge.wordpress.com</web>
// ****************************************************************************

using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;


// Usage:
//
// To make your WPF desktop application remember it's position and size on the screen between invocations,
// include this XML namespace-declaration and attribute within your Window's XAML:
//
// <Window x:Class="MyKicknApplication.MainWindow"
//     xmlns:jh="clr-namespace:Hurst;assembly=Hurst.BaseLib"
//     jh:WindowSettings.Save="True"
//
// To only save the Window position and not the size, use:
//
//     jh:WindowSettings.SavePosition="True"
//
// To not save the Window position, but save the size, use:
//
//     jh:WindowSettings.SaveSize="True"
//
// James W. Hurst,
// JamesH@Designforge.com

namespace MultiTemplateGenerator.UI.Helpers
{
    /// <summary>
    ///     Persists a Window's Size, Location and WindowState to UserScopeSettings
    /// </summary>
    public class WindowSettings
    {
        #region LoadWindowState

        /// <summary>
        ///     Load the Window Size Location and State from the settings object,
        ///     assigning that to the Window that's associated with this.
        /// </summary>
        protected virtual void LoadWindowState()
        {
            if (!Settings.IsUpgraded)
            {
                Settings.Upgrade();
                Settings.Reload();
                Settings.IsUpgraded = true;
            }

            // Deal with multiple monitors.
            if (Settings.Location != Rect.Empty)
            {
                if (_isSavingSize)
                {
                    _window.Width = Settings.Location.Width;
                    _window.Height = Settings.Location.Height;
                    //Console.WriteLine("in LoadWindowState, setting " + _sInstanceSettingsKey + " Width=" + Settings.Location.Width
                    //    + ", Height=" + Settings.Location.Height + ", _isSavingSizeOnly is " + _isSavingSizeOnly);
                }

                if (_isSavingPosition)
                {
                    _window.Left = Settings.Location.Left;
                    _window.Top = Settings.Location.Top;

                    // Apply a correction if the previous settings had it located on a monitor that no longer is available.
                    //
                    var virtualScreenTop = SystemParameters.VirtualScreenTop;
                    var virtualScreenWidth = SystemParameters.VirtualScreenWidth;
                    var virtualScreenHeight = SystemParameters.VirtualScreenHeight;
                    var virtualScreenLeft = SystemParameters.VirtualScreenLeft;
                    var virtualScreenRight = virtualScreenLeft + virtualScreenWidth;
                    var virtualScreenBottom = virtualScreenTop + virtualScreenHeight;
                    var myWidth = _window.Width;
                    var myBottom = _window.Top + _window.Height;

                    // If the 2nd monitor was to the right, and is now not..
                    if (_window.Left > virtualScreenRight - myWidth)
                    {
                        _window.Left = virtualScreenRight - myWidth;
                    }
                    // or if it was to the left..
                    else if (_window.Left < virtualScreenLeft)
                    {
                        _window.Left = virtualScreenLeft;
                    }
                    // or if there was a vertical change..
                    if (myBottom > virtualScreenBottom)
                    {
                        _window.Top = virtualScreenBottom - _window.Height;
                    }
                    else if (_window.Top < virtualScreenTop)
                    {
                        _window.Top = virtualScreenTop;
                    }
                }
            }

            if (_window.WindowState != WindowState.Minimized && Settings.WindowState != WindowState.Minimized)
            {
                _window.WindowState = Settings.WindowState;
            }
        }

        #endregion

        #region SaveWindowState

        /// <summary>
        ///     Save the Window Size, Location and State to the settings object
        /// </summary>
        protected virtual void SaveWindowState()
        {
            Settings.WindowState = _window.WindowState;
            Settings.Location = _window.RestoreBounds;
            Settings.Save();
        }

        #endregion

        #region Attach

        private void Attach()
        {
            if (_window != null)
            {
                _window.Closing += OnClosing;
                _window.Initialized += OnInitialized;
                _window.Loaded += OnLoaded;
            }
        }

        #endregion

        #region OnLoaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var setWinState = _window.Visibility == Visibility.Visible
                              && (_window.WindowState != WindowState.Minimized && Settings.WindowState != WindowState.Minimized);

            if (setWinState)
            {
                _window.WindowState = Settings.WindowState;
            }
        }

        #endregion

        #region OnInitialized

        private void OnInitialized(object sender, EventArgs e)
        {
            LoadWindowState();
        }

        #endregion

        #region OnClosing

        private void OnClosing(object sender, CancelEventArgs e)
        {
            SaveWindowState();
        }

        #endregion

        #region WindowApplicationSettings helper class

        public class WindowApplicationSettings : ApplicationSettingsBase
        {
            private readonly WindowSettings _windowSettings;

            public WindowApplicationSettings(WindowSettings windowSettings, string sInstanceKey)
                : base(sInstanceKey)
            {
                _windowSettings = windowSettings;
                //Set Default WindowState
                if (this[nameof(WindowState)] == null)
                {
                    this.WindowState = windowSettings._window.WindowState;
                }
            }

            private void WriteExceptionDiagnostics(ConfigurationErrorsException x)
            {
                // I added this for diagnosing a failure. See http://forums.msdn.microsoft.com/en-US/vbgeneral/thread/41cfc8e2-c7f4-462d-9a43-e751500deb0a
                Console.WriteLine(@"a ConfigurationErrorsException was raised within WindowSettings.Location.Get. Something may be wrong with your app config file!" +
                                  x);

                var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                Console.WriteLine(@"  exe config FilePath is " + exeConfig.FilePath);

                var localConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                Console.WriteLine(@"  local config FilePath is " + localConfig.FilePath);

                var roamingConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
                Console.WriteLine(@"  roaming config FilePath is " + roamingConfig.FilePath);
            }

            [UserScopedSetting]
            public bool IsUpgraded
            {
                get
                {
                    try
                    {
                        if (this[nameof(IsUpgraded)] != null)
                        {
                            return (bool)this[nameof(IsUpgraded)];
                        }
                    }
                    catch (ConfigurationErrorsException x)
                    {
                        WriteExceptionDiagnostics(x);
                    }

                    return false;
                }
                set => this[nameof(IsUpgraded)] = value;
            }

            [UserScopedSetting]
            public Rect Location
            {
                get
                {
                    try
                    {
                        if (this[nameof(Location)] != null)
                        {
                            return (Rect)this[nameof(Location)];
                        }
                    }
                    catch (ConfigurationErrorsException x)
                    {
                        WriteExceptionDiagnostics(x);
                    }
                    return Rect.Empty;
                }
                set => this[nameof(Location)] = value;
            }

            [UserScopedSetting]
            public WindowState WindowState
            {
                get
                {
                    try
                    {
                        if (this[nameof(WindowState)] != null)
                        {
                            return (WindowState)this[nameof(WindowState)];
                        }
                    }
                    catch (ConfigurationErrorsException x)
                    {
                        WriteExceptionDiagnostics(x);
                    }

                    return _windowSettings._window.WindowState;
                }
                set
                {
                    if (value != WindowState.Minimized)
                        this[nameof(WindowState)] = value;
                }
            }
        }

        #endregion

        #region Constructors

        // Provided for VS2008 since that does not support default parameter values.
        public WindowSettings(Window window)
        {
            _window = window;
            _isSavingSize = false;
            _isSavingPosition = true;
            // Use the class-name of the given Window, minus the namespace prefix, as the instance-key.
            // This is so that we have a distinct setting for different windows.
            var sWindowType = window.GetType().ToString();
            var iPos = sWindowType.LastIndexOf('.');
            var sKey = iPos > 0
                ? sWindowType.Substring(iPos + 1)
                : sWindowType;
            _sInstanceSettingsKey = !string.IsNullOrWhiteSpace(sKey) ? sKey : "MainWindow";
        }

        public WindowSettings(Window window, bool isSavingPosition, bool isSavingSize, string keyExtensionValue = null)
        {
            _window = window;
            _isSavingPosition = isSavingPosition;
            _isSavingSize = isSavingSize;
            // Use the class-name of the given Window, minus the namespace prefix, as the instance-key.
            // This is so that we have a distinct setting for different windows.
            var sWindowType = window.GetType().ToString();
            var iPos = sWindowType.LastIndexOf('.');
            var sKey = iPos > 0
                ? sWindowType.Substring(iPos + 1)
                : sWindowType;
            if (keyExtensionValue == null)
            {
                _sInstanceSettingsKey = sKey;
            }
            else
            {
                _sInstanceSettingsKey = sKey + keyExtensionValue;
            }
        }

        #endregion

        #region Attached "Save" Property Implementation

        /// <summary>
        ///     Register the "Save" attached property and the "OnSaveInvalidated" callback
        /// </summary>
        public static readonly DependencyProperty SaveProperty
            = DependencyProperty.RegisterAttached("Save", typeof(bool), typeof(WindowSettings),
                new FrameworkPropertyMetadata(OnSaveInvalidated));

        public static void SetSave(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(SaveProperty, enabled);
        }

        /// <summary>
        ///     Called when Save is changed on an object.
        /// </summary>
        private static void OnSaveInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is Window window && window.IsEnabled)
            {
                if ((bool)e.NewValue)
                {
                    var settings = new WindowSettings(window, true, true);
                    settings.Attach();
                }
            }
        }

        #endregion

        #region Attached "SavePosition" Property Implementation

        /// <summary>
        ///     Register the "SavePosition" attached property and the "OnSavePositionInvalidated" callback
        /// </summary>
        public static readonly DependencyProperty SavePositionProperty
            = DependencyProperty.RegisterAttached("SavePosition", typeof(bool), typeof(WindowSettings),
                new FrameworkPropertyMetadata(OnSavePositionInvalidated));

        public static void SetSavePosition(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(SavePositionProperty, enabled);
        }

        /// <summary>
        ///     Called when SavePosition is changed on an object.
        /// </summary>
        private static void OnSavePositionInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is Window window)
            {
                if ((bool)e.NewValue)
                {
                    var settings = new WindowSettings(window, true, false);
                    settings.Attach();
                }
            }
        }

        #endregion

        #region Attached "SaveSize" Property Implementation

        /// <summary>
        ///     Register the "SaveSize" attached property and the "OnSaveSizeInvalidated" callback
        /// </summary>
        public static readonly DependencyProperty SaveSizeProperty
            = DependencyProperty.RegisterAttached("SaveSize", typeof(bool), typeof(WindowSettings),
                new FrameworkPropertyMetadata(OnSaveSizeInvalidated));

        public static void SetSaveSize(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(SaveSizeProperty, enabled);
        }

        /// <summary>
        ///     Called when SaveSize is changed on an object.
        /// </summary>
        private static void OnSaveSizeInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is Window window)
            {
                if ((bool)e.NewValue)
                {
                    var settings = new WindowSettings(window, false, true);
                    settings.Attach();
                }
            }
        }

        #endregion

        #region Settings Property Implementation

        protected virtual WindowApplicationSettings CreateWindowApplicationSettingsInstance()
        {
            return new WindowApplicationSettings(this, _sInstanceSettingsKey);
        }

        [Browsable(false)]
        public WindowApplicationSettings Settings => _windowApplicationSettings
                                                     ?? (_windowApplicationSettings = CreateWindowApplicationSettingsInstance());

        #endregion

        #region fields

        private readonly Window _window;

        /// <summary>
        ///     This is used to dictate whether we're saving this Window's size, vs position+size, or just the position alone.
        /// </summary>
        private readonly bool _isSavingSize;

        /// <summary>
        ///     This is used to dictate whether we're saving this Window's position, vs position+size or just the size alone.
        /// </summary>
        private readonly bool _isSavingPosition;

        private WindowApplicationSettings _windowApplicationSettings;
        private readonly string _sInstanceSettingsKey;

        #endregion
    }
}