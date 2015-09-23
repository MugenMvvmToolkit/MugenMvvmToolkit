#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;
#if SILVERLIGHT
using System.Windows;
using MugenMvvmToolkit.Silverlight.Modules;

namespace MugenMvvmToolkit.Silverlight
#elif WINDOWS_PHONE
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Navigation;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.WinPhone.Interfaces;

namespace MugenMvvmToolkit.WinPhone
#endif

{
    public static class PlatformExtensions
    {
#if WINDOWS_PHONE
        private const string HandledPath = "#!~handled";
        private const string StatePath = "#!~vmstate";
        private static IApplicationStateManager _applicationStateManager;

        /// <summary>
        /// Gets or sets the <see cref="IApplicationStateManager"/>.
        /// </summary>
        [NotNull]
        public static IApplicationStateManager ApplicationStateManager
        {
            get
            {
                if (_applicationStateManager == null)
                    Interlocked.CompareExchange(ref _applicationStateManager, ServiceProvider.Get<IApplicationStateManager>(), null);
                return _applicationStateManager;
            }
            set { _applicationStateManager = value; }
        }

#elif !WINDOWS_PHONE
        #region Fields

        private static Func<Type, DataTemplate> _defaultViewModelTemplateFactory =
            ViewModelDataTemplateModule.DefaultTemplateProvider;

        #endregion

        #region Properties

        [NotNull]
        public static Func<Type, DataTemplate> DefaultViewModelTemplateFactory
        {
            get { return _defaultViewModelTemplateFactory; }
            set
            {
                Should.PropertyNotBeNull(value, "DefaultViewModelTemplateFactory");
                _defaultViewModelTemplateFactory = value;
            }
        }

        #endregion
#endif
        #region Methods

#if WINDOWS_PHONE
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

        internal static bool IsSerializable(this Type type)
        {
            return type.IsDefined(typeof(DataContractAttribute), false) || type.IsPrimitive;
        }
#endif

        internal static PlatformInfo GetPlatformInfo()
        {
#if WINDOWS_PHONE
            return new PlatformInfo(PlatformType.WinPhone, Environment.OSVersion.Version);
#else
            return new PlatformInfo(PlatformType.Silverlight, Environment.Version);
#endif
        }

        internal static void AsEventHandler<TArg>(this Action action, object sender, TArg arg)
        {
            action();
        }

        internal static NavigationMode ToNavigationMode(this System.Windows.Navigation.NavigationMode mode)
        {
            switch (mode)
            {
                case System.Windows.Navigation.NavigationMode.New:
                    return NavigationMode.New;
                case System.Windows.Navigation.NavigationMode.Back:
                    return NavigationMode.Back;
                case System.Windows.Navigation.NavigationMode.Forward:
                    return NavigationMode.Forward;
                case System.Windows.Navigation.NavigationMode.Refresh:
                    return NavigationMode.Refresh;
#if WINDOWS_PHONE8
                case System.Windows.Navigation.NavigationMode.Reset:
                    return NavigationMode.Reset;
#endif
                default:
                    return NavigationMode.Undefined;
            }
        }

        #endregion
    }
}