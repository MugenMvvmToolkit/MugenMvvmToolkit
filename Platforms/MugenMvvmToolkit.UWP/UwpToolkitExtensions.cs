#region Copyright

// ****************************************************************************
// <copyright file="UwpToolkitExtensions.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.UWP.Interfaces;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.UWP
{
    public static class UwpToolkitExtensions
    {
        #region Fields

        private static IApplicationStateManager _applicationStateManager;
        private const string HandledPath = "#!~handled";
        private const string StatePath = "#!~vmstate";

        #endregion

        #region Properties

        [NotNull]
        public static IApplicationStateManager ApplicationStateManager
        {
            get
            {
                if (_applicationStateManager == null)
                    _applicationStateManager = ServiceProvider.Get<IApplicationStateManager>();
                return _applicationStateManager;
            }
            set { _applicationStateManager = value; }
        }

        #endregion

        #region Methods

        public static bool GetHandled(this NavigationEventArgs args)
        {
            if (args.Content == null)
                return false;
            return ServiceProvider.AttachedValueProvider.GetValue<bool>(args.Content, HandledPath, false);
        }

        public static void SetHandled(this NavigationEventArgs args, bool handled)
        {
            if (args.Content == null) return;
            if (handled)
                ServiceProvider.AttachedValueProvider.SetValue(args.Content, HandledPath, Empty.TrueObject);
            else
                ServiceProvider.AttachedValueProvider.Clear(args.Content, HandledPath);
        }

        public static IDataContext GetViewModelState(object content)
        {
            if (content == null)
                return null;
            return ServiceProvider.AttachedValueProvider.GetValue<IDataContext>(content, StatePath, false);
        }

        public static void SetViewModelState(object content, IDataContext state)
        {
            if (content == null) return;
            if (state == null)
                ServiceProvider.AttachedValueProvider.Clear(content, StatePath);
            else
                ServiceProvider.AttachedValueProvider.SetValue(content, StatePath, state);
        }

        internal static void AsEventHandler<TArg>(this Action action, object sender, TArg arg)
        {
            action();
        }

        internal static PlatformInfo GetPlatformInfo()
        {
            // get the system version number
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var majorVersion = (version & 0xFFFF000000000000L) >> 48;
            var minorVersion = (version & 0x0000FFFF00000000L) >> 32;
            var buildVersion = (version & 0x00000000FFFF0000L) >> 16;
            var revisionVersion = version & 0x000000000000FFFFL;
            return new PlatformInfo(PlatformType.UWP, new Version((int)majorVersion, (int)minorVersion, (int)buildVersion, (int)revisionVersion).ToString(), GetIdiom);
        }

        private static PlatformIdiom GetIdiom()
        {
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Mobile":
                    return PlatformIdiom.Phone;
                case "Windows.Desktop":
                    return UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse
                        ? PlatformIdiom.Desktop
                        : PlatformIdiom.Tablet;
                default:
                    return PlatformIdiom.Unknown;
            }
        }

        internal static NavigationMode ToNavigationMode(this Windows.UI.Xaml.Navigation.NavigationMode mode)
        {
            switch (mode)
            {

                case Windows.UI.Xaml.Navigation.NavigationMode.New:
                    return NavigationMode.New;
                case Windows.UI.Xaml.Navigation.NavigationMode.Back:
                    return NavigationMode.Back;
                case Windows.UI.Xaml.Navigation.NavigationMode.Refresh:
                    return NavigationMode.Refresh;
                default:
                    return NavigationMode.Undefined;
            }
        }

        #endregion
    }
}
