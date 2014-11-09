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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit
{
    public static class PlatformExtensions
    {
#if WINDOWS_PHONE
#if V71
        //NOTE ConditionalWeakTable not supported on WP 7.8, we should keep references in memory.
        private static readonly List<WeakReference> WeakReferences;

        private sealed class WeakReferenceCollector
        {
            ~WeakReferenceCollector()
            {
                if (WeakReferences.Count == 0)
                    return;
                try
                {
                    lock (WeakReferences)
                    {
                        for (int i = 0; i < WeakReferences.Count; i++)
                        {
                            if (WeakReferences[i].Target == null)
                            {
                                WeakReferences.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
                finally
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

        static PlatformExtensions()
        {
            WeakReferences = new List<WeakReference>(64);
            // ReSharper disable once ObjectCreationAsStatement
            new WeakReferenceCollector();
        }
#endif

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
                    Interlocked.CompareExchange(ref _applicationStateManager,
                        ServiceProvider.IocContainer.Get<IApplicationStateManager>(), null);
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
                Should.PropertyBeNotNull(value, "DefaultViewModelTemplateFactory");
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
#if V71
        internal static WeakReference CreateWeakReference(object item, bool trackResurrection)
        {
            var reference = new WeakReference(item, trackResurrection);
            lock (WeakReferences)
                WeakReferences.Add(reference);
            return reference;
        }
#endif
#endif

        internal static PlatformInfo GetPlatformInfo()
        {
#if WINDOWS_PHONE
            return new PlatformInfo(PlatformType.WinPhone, Environment.OSVersion.Version);
#else
            return new PlatformInfo(PlatformType.Silverlight, Environment.Version);
#endif
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