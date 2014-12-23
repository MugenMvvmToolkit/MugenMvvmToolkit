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
using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    public static class PlatformExtensions
    {
        #region Fields

        private static Func<IComponent, string> _getComponentName;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            _getComponentName = GetComponentNameImpl;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the delegate that allows to get name from component.
        /// </summary>
        [NotNull]
        public static Func<IComponent, string> GetComponentName
        {
            get { return _getComponentName; }
            set { _getComponentName = value ?? GetComponentNameImpl; }
        }

        #endregion

        #region Methods

        public static IList<IDataBinding> SetBindings(this IComponent item, string bindingExpression,
             IList<object> sources = null)
        {
            return BindingServiceProvider.BindingProvider.CreateBindingsFromString(item, bindingExpression, sources);
        }

        public static T SetBindings<T, TBindingSet>([NotNull] this T item, [NotNull] TBindingSet bindingSet,
            [NotNull] string bindings)
            where T : IComponent
            where TBindingSet : BindingSet
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(bindingSet, "bindingSet");
            Should.NotBeNull(bindings, "bindings");
            bindingSet.BindFromExpression(item, bindings);
            return item;
        }


        public static T SetBindings<T, TBindingSet>([NotNull] this T item, [NotNull] TBindingSet bindingSet,
            [NotNull] Action<TBindingSet, T> setBinding)
            where T : IComponent
            where TBindingSet : BindingSet
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(bindingSet, "bindingSet");
            Should.NotBeNull(setBinding, "setBinding");
            setBinding(bindingSet, item);
            return item;
        }

        public static void ClearBindings([CanBeNull] this IComponent component, bool clearDataContext, bool clearAttachedValues)
        {
            BindingExtensions.ClearBindings(component, clearDataContext, clearAttachedValues);
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
        ///     Tries to get the root control.
        /// </summary>
        public static Control GetRootControl([CanBeNull] Control target)
        {
            Control root = target;
            while (target != null)
            {
                root = target;
                target = target.Parent;
            }
            return root;
        }

        internal static object SelectTemplateWithContext(this IDataTemplateSelector selector,
            [CanBeNull] object item, [NotNull] object container)
        {
            var template = selector.SelectTemplate(item, container);
            if (template != null && item != null)
                BindingServiceProvider.ContextManager.GetBindingContext(template).Value = item;
            return template;
        }

        internal static PlatformInfo GetPlatformInfo()
        {
#if WINFORMS
            return new PlatformInfo(PlatformType.WinForms, Environment.Version);
#endif
        }

        internal static string TryGetValue(object instance, string name)
        {
            if (instance == null)
                return null;
            IBindingMemberInfo member = BindingServiceProvider
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

        private static string GetComponentNameImpl(IComponent component)
        {
            if (component.Site == null)
                return TryGetValue(component, "Name");
            return component.Site.Name;
        }

        #endregion
    }
}