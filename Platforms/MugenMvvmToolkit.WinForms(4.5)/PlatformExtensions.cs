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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    public static class PlatformExtensions
    {
        #region Fields

        private static readonly Func<ReflectionExtensions.IWeakEventHandler<NotifyCollectionChangedEventArgs>, NotifyCollectionChangedEventHandler> CreateHandlerDelegate;
        private static readonly MethodInfo ToBindingListMethod;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            CreateHandlerDelegate = CreateHandler;
            ToBindingListMethod = typeof(PlatformExtensions).GetMethod("ToBindingList");
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an binding set.
        /// </summary>
        public static BindingSet<TTarget, TSource> CreateBindingSet<TTarget, TSource>([NotNull] this TTarget target,
            IBindingProvider bindingProvider = null) where TTarget : class, IComponent
        {
            Should.NotBeNull(target, "target");
            return new BindingSet<TTarget, TSource>(target, bindingProvider);
        }

        /// <summary>
        ///     Converts a collection to the <see cref="BindingListWrapper{T}" /> collection.
        /// </summary>
        /// <typeparam name="T">The type of collection.</typeparam>
        /// <param name="collection">The specified collection.</param>
        /// <returns>An instance of <see cref="BindingListWrapper{T}" />.</returns>
        public static BindingListWrapper<T> ToBindingList<T>(this SynchronizedNotifiableCollection<T> collection)
        {
            Should.NotBeNull(collection, "collection");
            return new BindingListWrapper<T>(collection);
        }

        /// <summary>
        ///     Tries to find the root control.
        /// </summary>
        public static Control FindRootControl([CanBeNull] Control target)
        {
            Control root = target;
            while (target != null)
            {
                root = target;
                target = target.Parent;
            }
            return root;
        }

        internal static PlatformInfo GetPlatformInfo()
        {
#if WINFORMS
            return new PlatformInfo(PlatformType.WinForms, Environment.Version);
#endif
        }

        public static NotifyCollectionChangedEventHandler MakeWeakCollectionChangedHandler<TTarget>(TTarget target,
            Action<TTarget, object, NotifyCollectionChangedEventArgs> invokeAction)
            where TTarget : class
        {
            return ReflectionExtensions.CreateWeakDelegate(target, invokeAction, UnsubscribeCollectionChanged, CreateHandlerDelegate);
        }

        internal static IList ToBindingListInternal(IList list)
        {
            Should.NotBeNull(list, "list");
            Type[] arguments = list.GetType().GetGenericArguments();
            return (IList)ToBindingListMethod.MakeGenericMethod(arguments).Invoke(null, new object[] { list });
        }

        private static void UnsubscribeCollectionChanged(object o, NotifyCollectionChangedEventHandler handler)
        {
            var notifyCollectionChanged = o as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                notifyCollectionChanged.CollectionChanged -= handler;
        }

        internal static string TryGetValue(object instance, string name)
        {
            if (instance == null)
                return null;
            var member = BindingProvider.Instance
                                        .MemberProvider
                                        .GetBindingMember(instance.GetType(), name, false, false);
            if (member == null || !member.CanRead)
                return null;

            object o = member.GetValue(instance, null);
            if (o == null)
                return null;
            return o.ToString();
        }

        internal static void Add(this SortedDictionary<string, AutoCompleteItem> dict, AutoCompleteItem item)
        {
            dict[item.Value] = item;
        }

        private static NotifyCollectionChangedEventHandler CreateHandler(ReflectionExtensions.IWeakEventHandler<NotifyCollectionChangedEventArgs> weakEventHandler)
        {
            return weakEventHandler.Handle;
        }

        #endregion
    }
}