#region Copyright

// ****************************************************************************
// <copyright file="PlatformBootstrapperService.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding;
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
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;

namespace MugenMvvmToolkit.Xamarin.Forms.WinRT
#endif
{
    internal sealed class PlatformBootstrapperService : XamarinFormsBootstrapperBase.IPlatformService
    {
        #region Constructors

        static PlatformBootstrapperService()
        {
            CompiledExpressionInvoker.SupportCoalesceExpression = false;
        }

        #endregion

        #region Methods

#if WINDOWSCOMMON
        private async System.Threading.Tasks.Task<List<Assembly>> GetAssemblyListAsync()
        {
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            List<Assembly> assemblies = new List<Assembly>();
            foreach (Windows.Storage.StorageFile file in await folder.GetFilesAsync().AsTask().ConfigureAwait(false))
            {
                if (file.FileType == ".dll" || file.FileType == ".exe")
                {
                    AssemblyName name = new AssemblyName { Name = Path.GetFileNameWithoutExtension(file.Name) };
                    Assembly asm = Assembly.Load(name);
                    assemblies.Add(asm);
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
            //NOTE: not a good solution but I do not know of another.
            var type = Type.GetType("Windows.Phone.ApplicationModel.ApplicationProfile, Windows, ContentType=WindowsRuntime", false);
            if (type == null)
                return new PlatformInfo(PlatformType.XamarinFormsWinRT, new Version(8, 1));
            return new PlatformInfo(PlatformType.XamarinFormsWinPhone, new Version(8, 1));
#endif

        }

        public ICollection<Assembly> GetAssemblies()
        {
#if WINDOWS_PHONE
            var listAssembly = new HashSet<Assembly>();
            foreach (var part in System.Windows.Deployment.Current.Parts)
            {
                string assemblyName = part.Source.Replace(".dll", string.Empty);
                if (assemblyName.Contains("/"))
                    continue;
                try
                {
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (assembly.IsToolkitAssembly())
                        listAssembly.Add(assembly);
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }
            return listAssembly;
#elif WINDOWSCOMMON
            return GetAssemblyListAsync().Result;
#else
            return new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies());
#endif
        }

        #endregion
    }
}
