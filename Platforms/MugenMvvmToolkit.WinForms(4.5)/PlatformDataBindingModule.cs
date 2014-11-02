#region Copyright
// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Converters;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit
{
    public class PlatformDataBindingModule : DataBindingModule
    {
        #region Fields

        internal readonly static IAttachedBindingMemberInfo<object, ICollectionViewManager> CollectionViewManagerMember;

        private readonly static IAttachedBindingMemberInfo<object, IContentViewManager> ContentViewManagerMember;
        private readonly static IAttachedBindingMemberInfo<object, IEnumerable> ItemsSourceMember;
        private readonly static IAttachedBindingMemberInfo<Control, object> ContentMember;
        private readonly static IAttachedBindingMemberInfo<Control, IDataTemplateSelector> ContentTemplateMember;

        #endregion

        #region Constructors

        static PlatformDataBindingModule()
        {
            //Object
            ItemsSourceMember = AttachedBindingMember.CreateAutoProperty<object, IEnumerable>(AttachedMemberConstants.ItemsSource, ObjectItemsSourceChanged);
            CollectionViewManagerMember = AttachedBindingMember.CreateAutoProperty<object, ICollectionViewManager>("CollectionViewManager");
            ContentViewManagerMember = AttachedBindingMember.CreateAutoProperty<object, IContentViewManager>("ContentViewManager");

            //Control
            ContentMember = AttachedBindingMember.CreateAutoProperty<Control, object>(AttachedMemberConstants.Content, ContentChanged);
            ContentTemplateMember = AttachedBindingMember.CreateAutoProperty<Control, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplate, ContentTemplateChanged);
        }

        #endregion

        #region Methods

        private static void Register([NotNull] IBindingMemberProvider memberProvider)
        {
            Should.NotBeNull(memberProvider, "memberProvider");

            //Object
            memberProvider.Register(AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.ItemsSource,
                    GetObjectItemsSource, SetObjectItemsSource, ObserveObjectItemsSource));

            memberProvider.Register(CollectionViewManagerMember);
            memberProvider.Register(ContentViewManagerMember);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplate));

            //Control
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, object>(AttachedMemberConstants.FindByNameMethod, FindByNameControlMember));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, bool>(AttachedMemberConstants.Focused, (info, control) => control.Focused, null, "LostFocus"));

            //Registering parent member as attached to avoid use the BindingExtensions.AttachedParentMember property.
            var parentMember = memberProvider.GetBindingMember(typeof(Control), AttachedMemberConstants.Parent, true, false);
            if (parentMember != null)
                memberProvider.Register(typeof(Control), parentMember, true);

            memberProvider.Register(ContentMember);
            memberProvider.Register(ContentTemplateMember);

            //DateTimePicker
            memberProvider.Register(AttachedBindingMember.CreateMember<DateTimePicker, DateTime>("Value",
                (info, picker) => picker.Value,
                (info, picker, value) =>
                {
                    if (value < picker.MinDate)
                        picker.Value = picker.MinDate;
                    else if (value > picker.MaxDate)
                        picker.Value = picker.MaxDate;
                    else
                        picker.Value = value;
                }, "ValueChanged"));

            //ToolStripItem
            memberProvider.Register(AttachedBindingMember.CreateMember<ToolStripItem, object>(AttachedMemberConstants.Parent,
                    GetParentToolStripItem, null, ObserveParentMemberToolStripItem));
            memberProvider.Register(AttachedBindingMember.CreateMember<ToolStripItem, object>(AttachedMemberConstants.FindByNameMethod,
                    FindByNameMemberToolStripItem));

            //TabControl
            memberProvider.Register(AttachedBindingMember.CreateMember<TabControl, object>(AttachedMemberConstants.SelectedItem,
                GetSelectedItemTabControl, SetSelectedItemTabControl, "Selected"));

            //ComboBox
            memberProvider.Register(
                AttachedBindingMember.CreateMember<ComboBox, object>(AttachedMemberConstants.ItemsSource,
                    (info, box) => box.DataSource,
                    (info, box, value) => box.DataSource = value, "DataSourceChanged"));
            memberProvider.Register(
                AttachedBindingMember.CreateMember<ComboBox, object>(AttachedMemberConstants.SelectedItem,
                    (info, box) => box.SelectedItem, (info, box, value) => box.SelectedItem = value,
                    "SelectedIndexChanged"));

            //DataGridView
            memberProvider.Register(
                AttachedBindingMember.CreateMember<DataGridView, object>(AttachedMemberConstants.ItemsSource,
                    (info, view) => view.DataSource, (info, view, value) =>
                    {
                        view.DataSource = value;
                        view.Refresh();
                    }, "DataSourceChanged"));
            memberProvider.Register(
                AttachedBindingMember.CreateMember<DataGridView, object>(AttachedMemberConstants.SelectedItem,
                    GetSelectedItemDataGridView, SetSelectedItemDataGridView, "CurrentCellChanged"));
        }

        #region Control

        private static void ContentTemplateChanged(Control control, AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(control, ContentMember.GetValue(control, null), args.NewValue);
        }

        private static void ContentChanged(Control control, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(control, args.NewValue, ContentTemplateMember.GetValue(control, null));
        }

        private static void UpdateContent(Control container, object value, IDataTemplateSelector selector)
        {
            if (selector != null)
                value = selector.SelectTemplateWithContext(value, container);
            var content = value as Control;
            if (content == null)
            {
                var viewModel = value as IViewModel;
                if (viewModel != null)
                    content = ViewModelToViewConverter.Instance.Convert(viewModel) as Control;
            }
            if (content == null && value != null)
            {
                Tracer.Warn("The content value {0} is not a Control.", value);
                content = new TextBox
                {
                    ReadOnly = true,
                    Text = value.ToString(),
                    Multiline = true
                };

            }
            IContentViewManager viewManager = ContentViewManagerMember.GetValue(container, null);
            if (viewManager == null)
            {
                container.Controls.Clear();
                if (content != null)
                {
                    content.Dock = DockStyle.Fill;
                    content.AutoSize = true;
                    container.Size = content.Size;
                    container.Controls.Add(content);
                }
            }
            else
                viewManager.SetContent(container, content);
        }

        private static object FindByNameControlMember(IBindingMemberInfo bindingMemberInfo, Control control, object[] arg3)
        {
            var root = PlatformExtensions.GetRootControl(control);
            if (root != null)
                control = root;
            return control.Controls.Find((string)arg3[0], true).FirstOrDefault();
        }

        #endregion

        #region Object

        private static IDisposable ObserveObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, IEventListener arg3)
        {
            return GetObjectItemsSourceMember(component).TryObserve(component, arg3);
        }

        private static object SetObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, object[] arg3)
        {
            return GetObjectItemsSourceMember(component).SetValue(component, arg3);
        }

        private static object GetObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, object[] arg3)
        {
            return GetObjectItemsSourceMember(component).GetValue(component, arg3);
        }

        private static void ObjectItemsSourceChanged(object control, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            ItemsSourceGenerator.GetOrAdd(control).SetItemsSource(args.NewValue);
        }

        private static IBindingMemberInfo GetObjectItemsSourceMember(object component)
        {
            return BindingServiceProvider.MemberProvider.GetBindingMember(component.GetType(),
                AttachedMemberConstants.ItemsSource, true, false) ?? ItemsSourceMember;
        }

        #endregion

        #region ToolStripItem

        private static object FindByNameMemberToolStripItem(IBindingMemberInfo bindingMemberInfo, ToolStripItem target, object[] arg3)
        {
            Control control = GetOwner(target);
            if (control == null)
                return null;
            return FindByNameControlMember(null, control, arg3);
        }

        private static object GetParentToolStripItem(IBindingMemberInfo bindingMemberInfo, ToolStripItem target)
        {
            return GetOwner(target);
        }

        private static IDisposable ObserveParentMemberToolStripItem(IBindingMemberInfo bindingMemberInfo, ToolStripItem toolStripItem, IEventListener arg3)
        {
            EventHandler handler = arg3.ToWeakEventListener().Handle;
            ToolStrip owner = GetOwner(toolStripItem);
            WeakReference ownerRef = null;
            if (owner != null)
            {
                owner.ParentChanged += handler;
                ownerRef = ServiceProvider.WeakReferenceFactory(owner, true);
            }
            toolStripItem.OwnerChanged += handler;
            var menuItemRef = ServiceProvider.WeakReferenceFactory(toolStripItem, true);
            return new ActionToken(() =>
            {
                if (ownerRef != null)
                {
                    var toolStrip = ownerRef.Target as ToolStrip;
                    if (toolStrip != null)
                        toolStrip.ParentChanged -= handler;
                    ownerRef = null;
                }
                var item = menuItemRef.Target as ToolStripItem;
                if (item != null)
                    item.OwnerChanged -= handler;
                menuItemRef = null;
            });
        }

        private static ToolStrip GetOwner(ToolStripItem menuItem)
        {
            ToolStrip owner = menuItem.Owner;
            while (owner is ToolStripDropDownMenu)
                owner = (owner as ToolStripDropDownMenu).OwnerItem.Owner;
            return owner;
        }

        #endregion

        #region DataGridView

        private static void SetSelectedItemDataGridView(IBindingMemberInfo bindingMemberInfo, DataGridView dataGridView, object item)
        {
            dataGridView.ClearSelection();
            if (item == null)
                return;
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if (Equals(dataGridView.Rows[i].DataBoundItem, item))
                {
                    var row = dataGridView.Rows[i];
                    row.Selected = true;
                    if (row.Cells.Count > 0)
                        row.Cells[0].Selected = true;
                    break;
                }
            }
        }

        private static object GetSelectedItemDataGridView(IBindingMemberInfo bindingMemberInfo, DataGridView dataGridView)
        {
            var row = dataGridView.CurrentRow;
            if (row == null)
                return null;
            return row.DataBoundItem;
        }

        #endregion

        #region TabControl

        private static object GetSelectedItemTabControl(IBindingMemberInfo bindingMemberInfo, TabControl tabControl)
        {
            if (tabControl.TabCount == 0 || tabControl.SelectedIndex < 0)
                return null;
            return BindingServiceProvider
                .ContextManager
                .GetBindingContext(tabControl.TabPages[tabControl.SelectedIndex]).Value;
        }

        private static void SetSelectedItemTabControl(IBindingMemberInfo bindingMemberInfo, TabControl tabControl, object item)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (Equals(BindingServiceProvider.ContextManager.GetBindingContext(tabPage).Value, item))
                {
                    tabControl.SelectedTab = tabPage;
                    break;
                }
            }
        }

        #endregion

        #endregion

        #region Overrides of DataBindingModule

        /// <summary>
        ///    Occurs on load the current module.
        /// </summary>
        protected override void OnLoaded(IModuleContext context)
        {
            Register(BindingServiceProvider.MemberProvider);
            base.OnLoaded(context);
        }

        /// <summary>
        ///     Gets the <see cref="IBindingErrorProvider" /> that will be used by default.
        /// </summary>
        protected override IBindingErrorProvider GetBindingErrorProvider()
        {
            return new BindingErrorProvider();
        }

        #endregion
    }
}