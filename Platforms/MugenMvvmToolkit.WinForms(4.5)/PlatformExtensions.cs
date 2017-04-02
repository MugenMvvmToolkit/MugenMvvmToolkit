#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.WinForms.Binding;
using MugenMvvmToolkit.WinForms.Binding.Models;
using MugenMvvmToolkit.WinForms.Collections;

namespace MugenMvvmToolkit.WinForms
{
    public static partial class PlatformExtensions
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

        [NotNull]
        public static Func<IComponent, string> GetComponentName
        {
            get { return _getComponentName; }
            set { _getComponentName = value ?? GetComponentNameImpl; }
        }

        #endregion

        #region Methods

        public static BindingListWrapper<T> ToBindingList<T>(this INotifiableCollection<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            return new BindingListWrapper<T>(collection);
        }

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
            if (template != null)
            {
                template.SetDataContext(item);
                if (!(template is Control) && !(template is ToolStripItem) && template.GetBindingMemberValue(AttachedMembers.Object.Parent) == null)
                    template.SetBindingMemberValue(AttachedMembers.Object.Parent, container);
            }
            return template;
        }

        internal static PlatformInfo GetPlatformInfo()
        {
            return new PlatformInfo(PlatformType.WinForms, Environment.Version.ToString(), PlatformIdiom.Desktop);
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
            return member.GetValue(instance, null)?.ToString();
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
