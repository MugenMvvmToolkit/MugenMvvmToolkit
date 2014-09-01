#region Copyright
// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    /// <summary>
    /// Represents the platform specific extensions.
    /// </summary>
    public static class PlatformExtensions
    {
        #region Fields

        private static IApplicationStateManager _applicationStateManager;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IApplicationStateManager"/>.
        /// </summary>
        [NotNull]
        public static IApplicationStateManager ApplicationStateManager
        {
            get
            {
                if (_applicationStateManager == null)
                    Interlocked.CompareExchange(ref _applicationStateManager,
                        new ApplicationStateManager(ServiceProvider.IocContainer.Get<ISerializer>()), null);
                return _applicationStateManager;
            }
            set { _applicationStateManager = value; }
        }

        #endregion

        #region Methods

        internal static bool IsSerializable(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsDefined(typeof(DataContractAttribute), false) || typeInfo.IsPrimitive;
        }

        internal static PlatformInfo GetPlatformInfo()
        {
#if NETFX_CORE
            return new PlatformInfo(PlatformType.WinRT, new Version(8, 0));
#else
            //NOTE: not a good solution but I do not know of another.
            var type = Type.GetType("Windows.Phone.ApplicationModel.ApplicationProfile, Windows, ContentType=WindowsRuntime", false);
            if (type == null)
                return new PlatformInfo(PlatformType.WinRT, new Version(8, 1));
            return new PlatformInfo(PlatformType.WinPhone, new Version(8, 1));
#endif
        }

        internal static NavigationMode ToNavigationMode(this Windows.UI.Xaml.Navigation.NavigationMode mode)
        {
            switch (mode)
            {

                case Windows.UI.Xaml.Navigation.NavigationMode.New:
                    return NavigationMode.New;
                case Windows.UI.Xaml.Navigation.NavigationMode.Back:
                    return NavigationMode.Back;
                case Windows.UI.Xaml.Navigation.NavigationMode.Forward:
                    return NavigationMode.Forward;
                case Windows.UI.Xaml.Navigation.NavigationMode.Refresh:
                    return NavigationMode.Refresh;
                default:
                    return NavigationMode.Undefined;
            }
        }

        #endregion
    }
}