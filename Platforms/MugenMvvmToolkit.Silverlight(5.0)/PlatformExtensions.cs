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
using System.Windows;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit
{
    public static class PlatformExtensions
    {
#if WINDOWS_PHONE && V78
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
                            if (!WeakReferences[i].IsAlive)
                                WeakReferences.RemoveAt(i);
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
        internal static bool IsSerializable(this Type type)
        {
            return type.IsDefined(typeof(DataContractAttribute), false) || type.IsPrimitive;
        }

#if V78
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

        public static NotifyCollectionChangedEventHandler MakeWeakCollectionChangedHandler<TTarget>(TTarget target,
            Action<TTarget, object, NotifyCollectionChangedEventArgs> invokeAction, bool cacheWeakReferenceTarget)
            where TTarget : class
        {
            return ReflectionExtensions
                .CreateWeakDelegate<TTarget, NotifyCollectionChangedEventArgs, NotifyCollectionChangedEventHandler>(
                    target, invokeAction, UnsubscribeCollectionChanged, handler => handler.Handle,
                    cacheWeakReferenceTarget);
        }

        private static void UnsubscribeCollectionChanged(object o, NotifyCollectionChangedEventHandler handler)
        {
            var notifyCollectionChanged = o as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                notifyCollectionChanged.CollectionChanged -= handler;
        }

        #endregion
    }
}