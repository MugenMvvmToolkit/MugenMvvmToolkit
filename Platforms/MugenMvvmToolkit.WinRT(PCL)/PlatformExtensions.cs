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
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.WinRT.Interfaces;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.WinRT
{
    public static class PlatformExtensions
    {
        #region Fields

        private static IApplicationStateManager _applicationStateManager;
        private const string HandledPath = "#!~handled";
        private const string StatePath = "#!~vmstate";
        private static PropertyInfo BackArgsHandledProperty;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            SubscribeBackPressedEventDelegate = (o, action) =>
            {
                try
                {
                    var type = Type.GetType("Windows.Phone.UI.Input.HardwareButtons, Windows, ContentType=WindowsRuntime", false);
                    if (type == null)
                        return;
                    var eventInfo = type.GetEventEx("BackPressed", MemberFlags.Public | MemberFlags.Static);
                    var handleMethod = typeof(ReflectionExtensions.IWeakEventHandler<object>).GetMethodEx("Handle", MemberFlags.Public | MemberFlags.Instance);
                    if (eventInfo == null || handleMethod == null || eventInfo.AddMethod == null)
                        return;
                    object token = null;
                    var handler = ReflectionExtensions.CreateWeakEventHandler(o, action, (o1, h) =>
                    {
                        if (token != null && eventInfo.RemoveMethod != null)
                            eventInfo.RemoveMethod.Invoke(null, new[] { token });
                    });
                    var @delegate = handleMethod.CreateDelegate(eventInfo.EventHandlerType, handler);
                    token = eventInfo.AddMethod.Invoke(null, new object[] { @delegate });
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(false));
                }
            };
            SetBackPressedHandledDelegate = (o, b) =>
            {
                if (BackArgsHandledProperty == null)
                    BackArgsHandledProperty = o.GetType().GetPropertyEx("Handled", MemberFlags.Public | MemberFlags.Instance);
                if (BackArgsHandledProperty != null)
                    BackArgsHandledProperty.SetValue(o, Empty.BooleanToObject(b));
            };
        }

        #endregion

        #region Properties

        [CanBeNull]
        public static Action<object, Action<object, object, object>> SubscribeBackPressedEventDelegate { get; set; }

        [CanBeNull]
        public static Action<object, bool> SetBackPressedHandledDelegate { get; set; }

        [NotNull]
        public static IApplicationStateManager ApplicationStateManager
        {
            get
            {
                if (_applicationStateManager == null)
                    Interlocked.CompareExchange(ref _applicationStateManager,
                        ServiceProvider.Get<IApplicationStateManager>(), null);
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

        internal static bool IsSerializable(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsDefined(typeof(DataContractAttribute), false) || typeInfo.IsPrimitive;
        }

        internal static PlatformInfo GetPlatformInfo()
        {
            //NOTE: not a good solution but I do not know of another.
            var isPhone = new EasClientDeviceInformation().OperatingSystem.SafeContains("WindowsPhone", StringComparison.OrdinalIgnoreCase);
            var isWinRT10 = typeof(DependencyObject).GetMethodEx("RegisterPropertyChangedCallback", MemberFlags.Instance | MemberFlags.Public) != null;
            var version = isWinRT10 ? new Version(10, 0) : new Version(8, 1);
            return new PlatformInfo(isPhone ? PlatformType.WinRTPhone : PlatformType.WinRT, version);
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
