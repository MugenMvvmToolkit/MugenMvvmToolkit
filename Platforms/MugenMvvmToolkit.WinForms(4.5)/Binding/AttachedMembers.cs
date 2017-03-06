#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembers.cs">
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

using System.Collections;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.WinForms.Binding.Infrastructure;
using MugenMvvmToolkit.WinForms.Binding.Interfaces;

namespace MugenMvvmToolkit.WinForms.Binding
{
    public static class AttachedMembers
    {
        #region Nested types

        public abstract class Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<object, object> DataContext;
            public static BindingMemberDescriptor<object, object> Parent;
            public static readonly BindingMemberDescriptor<object, object> CommandParameter;
            public static readonly BindingMemberDescriptor<object, IEnumerable<object>> Errors;

            public static readonly BindingMemberDescriptor<object, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<object, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<object, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<object, ICollectionViewManager> CollectionViewManager;

            #endregion

            #region Constructors

            static Object()
            {
                DataContext = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.DataContext);
                Parent = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.Parent);
                CommandParameter = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.CommandParameter);
                Errors = new BindingMemberDescriptor<object, IEnumerable<object>>(AttachedMemberConstants.ErrorsPropertyMember);

                ItemsSource = new BindingMemberDescriptor<object, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemsSourceGenerator = new BindingMemberDescriptor<object, IItemsSourceGenerator>(ItemsSourceGeneratorBase.MemberDescriptor);
                ItemTemplateSelector = new BindingMemberDescriptor<object, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                CollectionViewManager = new BindingMemberDescriptor<object, ICollectionViewManager>(nameof(CollectionViewManager));
            }

            #endregion
        }

        public abstract class Form : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<System.Windows.Forms.Form, IDataTemplateSelector> ToastTemplateSelector;

            #endregion

            #region Constructors

            static Form()
            {
                ToastTemplateSelector = new BindingMemberDescriptor<System.Windows.Forms.Form, IDataTemplateSelector>(nameof(ToastTemplateSelector));
            }

            #endregion
        }

        public abstract class Control : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<System.Windows.Forms.Control, object> Content;
            public static readonly BindingMemberDescriptor<System.Windows.Forms.Control, IContentViewManager> ContentViewManager;
            public static readonly BindingMemberDescriptor<System.Windows.Forms.Control, IDataTemplateSelector> ContentTemplateSelector;

            #endregion

            #region Constructors

            static Control()
            {
                Content = new BindingMemberDescriptor<System.Windows.Forms.Control, object>(AttachedMemberConstants.Content);
                ContentTemplateSelector = new BindingMemberDescriptor<System.Windows.Forms.Control, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplateSelector);
                ContentViewManager = new BindingMemberDescriptor<System.Windows.Forms.Control, IContentViewManager>(nameof(ContentViewManager));
            }

            #endregion
        }

        public abstract class TabControl : Control
        {
            #region Fields

            public static readonly BindingMemberDescriptor<System.Windows.Forms.TabControl, object> SelectedItem;

            #endregion

            #region Constructors

            static TabControl()
            {
                SelectedItem = new BindingMemberDescriptor<System.Windows.Forms.TabControl, object>(AttachedMemberConstants.SelectedItem);
            }

            #endregion
        }

        public abstract class ComboBox : Control
        {
            #region Fields

            public static readonly BindingMemberDescriptor<System.Windows.Forms.ComboBox, object> SelectedItem;

            #endregion

            #region Constructors

            static ComboBox()
            {
                SelectedItem = new BindingMemberDescriptor<System.Windows.Forms.ComboBox, object>(AttachedMemberConstants.SelectedItem);
            }

            #endregion
        }

        public abstract class DataGridView : Control
        {
            #region Fields

            public static readonly BindingMemberDescriptor<System.Windows.Forms.DataGridView, object> SelectedItem;

            #endregion

            #region Constructors

            static DataGridView()
            {
                SelectedItem = new BindingMemberDescriptor<System.Windows.Forms.DataGridView, object>(AttachedMemberConstants.SelectedItem);
            }

            #endregion
        }

        #endregion
    }
}
