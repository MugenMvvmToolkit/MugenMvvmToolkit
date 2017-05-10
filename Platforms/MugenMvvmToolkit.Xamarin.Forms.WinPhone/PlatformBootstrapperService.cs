#region Copyright

// ****************************************************************************
// <copyright file="PlatformBootstrapperService.cs">
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
using System.Collections.Generic;
using System.Reflection;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Xamarin.Forms.Binding;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure;

#if WINDOWS_PHONE
namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone
#elif TOUCH
namespace MugenMvvmToolkit.Xamarin.Forms.iOS
#elif ANDROID
using Android.Content;
namespace MugenMvvmToolkit.Xamarin.Forms.Android
#elif WINDOWS_UWP
using MugenMvvmToolkit.Models.Messages;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.ApplicationModel;
using Windows.UI.ViewManagement;
using Windows.System.Profile;

namespace MugenMvvmToolkit.Xamarin.Forms.UWP
#elif NETFX_CORE
using System.IO;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;

namespace MugenMvvmToolkit.Xamarin.Forms.WinRT
#endif
{
    public class PlatformBootstrapperService : XamarinFormsBootstrapperBase.IPlatformService
    {
        #region Constructors
#if ANDROID
        public PlatformBootstrapperService(Func<Context> getCurrentContext)
        {
            XamarinFormsAndroidToolkitExtensions.GetCurrentContext = getCurrentContext;
        }
#endif
        #endregion

        #region Methods

#if WINDOWS_UWP
        private static void OnLeavingBackground(object sender, LeavingBackgroundEventArgs leavingBackgroundEventArgs)
        {
            ServiceProvider.Application?.SetApplicationState(ApplicationState.Active, null);
        }

        private static void OnEnteredBackground(object sender, EnteredBackgroundEventArgs enteredBackgroundEventArgs)
        {
            ServiceProvider.Application?.SetApplicationState(ApplicationState.Background, null);
        }
#endif
        #endregion

        #region Implementation of IPlatformService

        public PlatformInfo GetPlatformInfo()
        {
#if WINDOWS_PHONE
            return new PlatformInfo(PlatformType.XamarinFormsWinPhone, Environment.OSVersion.Version.ToString(), PlatformIdiom.Phone);
#elif TOUCH
            return new PlatformInfo(PlatformType.XamarinFormsiOS, UIKit.UIDevice.CurrentDevice.SystemVersion, GetIdiom);
#elif ANDROID
            return new PlatformInfo(PlatformType.XamarinFormsAndroid, global::Android.OS.Build.VERSION.Release, GetIdiom);
#elif WINDOWS_UWP
            // get the system version number
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var majorVersion = (version & 0xFFFF000000000000L) >> 48;
            var minorVersion = (version & 0x0000FFFF00000000L) >> 32;
            var buildVersion = (version & 0x00000000FFFF0000L) >> 16;
            var revisionVersion = version & 0x000000000000FFFFL;
            return new PlatformInfo(PlatformType.XamarinFormsUWP, new Version((int)majorVersion, (int)minorVersion, (int)buildVersion, (int)revisionVersion).ToString(), GetIdiom);
#elif NETFX_CORE
            var isWinRT10 = typeof(DependencyObject).GetMethodEx("RegisterPropertyChangedCallback", MemberFlags.Instance | MemberFlags.Public) != null;
            var version = isWinRT10 ? new Version(10, 0) : new Version(8, 1);

#if WINDOWS_PHONE_APP
            return new PlatformInfo(PlatformType.XamarinFormsWinRT, version.ToString(), PlatformIdiom.Phone);
#else
            return new PlatformInfo(PlatformType.XamarinFormsWinRT, version.ToString(), PlatformIdiom.Tablet);
#endif
#endif
        }

        public ICollection<Assembly> GetAssemblies()
        {
#if WINDOWS_PHONE
            var assemblies = new HashSet<Assembly>();
            foreach (var part in System.Windows.Deployment.Current.Parts)
            {
                string assemblyName = part.Source.Replace(".dll", string.Empty);
                if (assemblyName.Contains("/"))
                    continue;
                try
                {
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }
            return assemblies;
#elif WINDOWS_UWP || NETFX_CORE
            return new[] { typeof(PlatformBootstrapperService).GetAssembly() };
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }

        private static PlatformIdiom GetIdiom()
        {
#if ANDROID
            var context = XamarinFormsAndroidToolkitExtensions.GetContext();
            int minWidthDp = context.Resources.Configuration.SmallestScreenWidthDp;
            return minWidthDp >= 600 ? PlatformIdiom.Tablet : PlatformIdiom.Phone;
#elif TOUCH
            return XamarinFormsTouchToolkitExtensions.GetIdiom();
#elif WINDOWS_UWP
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
#else
            return PlatformIdiom.Unknown;
#endif
        }

        public void Initialize()
        {
#if WINDOWS_UWP
            var application = Application.Current;
            if (application != null)
            {
                if (ApiInformation.IsEventPresent("Windows.UI.Xaml.Application", "EnteredBackground"))
                    application.EnteredBackground += OnEnteredBackground;
                if (ApiInformation.IsEventPresent("Windows.UI.Xaml.Application", "LeavingBackground"))
                    application.LeavingBackground += OnLeavingBackground;
            }
#endif
        }

        public Func<MemberInfo, Type, object, object> ValueConverter => BindingConverterExtensions.Convert;

        #endregion
    }
}
