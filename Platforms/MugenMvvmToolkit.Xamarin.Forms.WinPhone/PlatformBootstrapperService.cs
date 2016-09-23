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
#elif WINDOWSCOMMON
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

#if WINDOWSCOMMON
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
#if WINDOWSCOMMON
                return BindingServiceProvider.ValueConverter;
#else
                return BindingReflectionExtensions.Convert;
#endif
            }
        }

        public PlatformInfo GetPlatformInfo()
        {
#if WINDOWS_PHONE
            return new PlatformInfo(PlatformType.XamarinFormsWinPhone, Environment.OSVersion.Version);
#elif TOUCH
            Version result;
            Version.TryParse(UIKit.UIDevice.CurrentDevice.SystemVersion, out result);
            return new PlatformInfo(PlatformType.XamarinFormsiOS, result);
#elif ANDROID
            Version result;
            Version.TryParse(global::Android.OS.Build.VERSION.Release, out result);
            return new PlatformInfo(PlatformType.XamarinFormsAndroid, result);
#elif WINDOWSCOMMON
            var isPhone = new EasClientDeviceInformation().OperatingSystem.SafeContains("WindowsPhone", StringComparison.OrdinalIgnoreCase);
            var isWinRT10 = typeof(DependencyObject).GetMethodEx("RegisterPropertyChangedCallback", MemberFlags.Instance | MemberFlags.Public) != null;
            var version = isWinRT10 ? new Version(10, 0) : new Version(8, 1);
            return new PlatformInfo(isPhone ? PlatformType.XamarinFormsWinRTPhone : PlatformType.XamarinFormsWinRT, version);
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
#elif WINDOWSCOMMON
            return GetAssemblyListAsync().Result;
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }

        #endregion
    }
}
