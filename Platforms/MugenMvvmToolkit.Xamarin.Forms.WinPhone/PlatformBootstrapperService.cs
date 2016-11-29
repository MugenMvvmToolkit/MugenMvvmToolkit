#region Copyright

// ****************************************************************************
// <copyright file="PlatformBootstrapperService.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure;

#if WINDOWS_PHONE
namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone
#elif TOUCH
namespace MugenMvvmToolkit.Xamarin.Forms.iOS
#elif ANDROID
namespace MugenMvvmToolkit.Xamarin.Forms.Android
#elif WINDOWS_UWP
using MugenMvvmToolkit.Binding;
using System.IO;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace MugenMvvmToolkit.Xamarin.Forms.UWP
#elif NETFX_CORE
using MugenMvvmToolkit.Binding;
using System.IO;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;

namespace MugenMvvmToolkit.Xamarin.Forms.WinRT
#endif
{
    public sealed class PlatformBootstrapperService : XamarinFormsBootstrapperBase.IPlatformService
    {
        #region Constructors

        static PlatformBootstrapperService()
        {
            CompiledExpressionInvoker.SupportCoalesceExpression = false;
        }

        #endregion

        #region Methods

#if WINDOWS_UWP || NETFX_CORE
        private static async System.Threading.Tasks.Task<HashSet<Assembly>> GetAssemblyListAsync()
        {
            var assemblies = new HashSet<Assembly>();
            var files = await Package.Current.InstalledLocation.GetFilesAsync().AsTask().ConfigureAwait(false);
            foreach (var file in files)
            {
                try
                {
                    if ((file.FileType == ".dll") || (file.FileType == ".exe"))
                    {
                        var name = new AssemblyName { Name = Path.GetFileNameWithoutExtension(file.Name) };
                        assemblies.Add(Assembly.Load(name));
                    }

                }
                catch
                {
                    ;
                }
            }
            return assemblies;
        }
#endif
        #endregion

        #region Implementation of IPlatformService

        public Func<IBindingMemberInfo, Type, object, object> ValueConverter
        {
            get
            {
#if WINDOWS_UWP || NETFX_CORE
                return BindingServiceProvider.ValueConverter;
#else
                return BindingReflectionExtensions.Convert;
#endif
            }
        }

        public PlatformInfo GetPlatformInfo()
        {
#if WINDOWS_PHONE
            return new PlatformInfo(PlatformType.XamarinFormsWinPhone, Environment.OSVersion.Version.ToString());
#elif TOUCH            
            return new PlatformInfo(PlatformType.XamarinFormsiOS, UIKit.UIDevice.CurrentDevice.SystemVersion);
#elif ANDROID            
            return new PlatformInfo(PlatformType.XamarinFormsAndroid, global::Android.OS.Build.VERSION.Release);
#elif WINDOWS_UWP
            // get the system version number
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var majorVersion = (version & 0xFFFF000000000000L) >> 48;
            var minorVersion = (version & 0x0000FFFF00000000L) >> 32;
            var buildVersion = (version & 0x00000000FFFF0000L) >> 16;
            var revisionVersion = (version & 0x000000000000FFFFL);
            var isPhone = new EasClientDeviceInformation().OperatingSystem.SafeContains("WindowsPhone", StringComparison.OrdinalIgnoreCase);
            return new PlatformInfo(isPhone ? PlatformType.XamarinFormsUWPPhone : PlatformType.XamarinFormsUWP, new Version((int)majorVersion, (int)minorVersion, (int)buildVersion, (int)revisionVersion).ToString());
#elif NETFX_CORE            
            var isPhone = new EasClientDeviceInformation().OperatingSystem.SafeContains("WindowsPhone", StringComparison.OrdinalIgnoreCase);
            var isWinRT10 = typeof(DependencyObject).GetMethodEx("RegisterPropertyChangedCallback", MemberFlags.Instance | MemberFlags.Public) != null;
            var version = isWinRT10 ? new Version(10, 0) : new Version(8, 1);
            return new PlatformInfo(isPhone ? PlatformType.XamarinFormsWinRTPhone : PlatformType.XamarinFormsWinRT, version.ToString());
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
            return GetAssemblyListAsync().Result;
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }

        #endregion
    }
}
