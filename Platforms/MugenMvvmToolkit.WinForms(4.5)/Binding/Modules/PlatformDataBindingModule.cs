#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
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
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.WinForms.Binding.Converters;
using MugenMvvmToolkit.WinForms.Binding.Infrastructure;
using MugenMvvmToolkit.WinForms.Binding.Interfaces;

namespace MugenMvvmToolkit.WinForms.Binding.Modules
{
    public class PlatformDataBindingModule : DataBindingModule
    {
        #region Methods

        private static void Register([NotNull] IBindingMemberProvider memberProvider)
        {
            Should.NotBeNull(memberProvider, "memberProvider");

            //Object
            var itemsSourceMember = AttachedBindingMember.CreateAutoProperty<object, IEnumerable>(AttachedMemberConstants.ItemsSource, ObjectItemsSourceChanged);
            var defaultMemberRegistration = new DefaultAttachedMemberRegistration<IEnumerable>(itemsSourceMember);
            memberProvider.Register(defaultMemberRegistration.ToAttachedBindingMember<object>());
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, ICollectionViewManager>(AttachedMembers.Control.CollectionViewManager.Path));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IContentViewManager>(AttachedMembers.Control.ContentViewManager.Path));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(ItemsSourceGeneratorBase.MemberDescriptor,
                (o, args) =>
                {
                    IEnumerable itemsSource = null;
                    if (args.OldValue != null)
                    {
                        itemsSource = args.OldValue.ItemsSource;
                        args.OldValue.SetItemsSource(null);
                    }
                    if (args.NewValue != null)
                        args.NewValue.SetItemsSource(itemsSource);
                }));

            var itemTemplateMember = AttachedBindingMember.CreateAutoProperty<object, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
            memberProvider.Register(itemTemplateMember);
            memberProvider.Register(typeof(object), AttachedMemberConstants.ItemTemplate, itemTemplateMember, true);

            //Control
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, object>(AttachedMemberConstants.FindByNameMethod, FindByNameControlMember));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, bool>(AttachedMemberConstants.Focused, (info, control) => control.Focused,
                    (info, control, arg3) =>
                    {
                        if (arg3)
                            control.Focus();
                    }, "LostFocus"));

            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Control.Content, ContentChanged));
            var contenMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.Control.ContentTemplateSelector, ContentTemplateChanged);
            memberProvider.Register(contenMember);
            memberProvider.Register(typeof(Control), AttachedMemberConstants.ContentTemplate, contenMember, true);

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
            memberProvider.Register(AttachedBindingMember.CreateMember<ToolStripItem, object>(AttachedMemberConstants.ParentExplicit,
                    GetParentToolStripItem, null, ObserveParentMemberToolStripItem));
            memberProvider.Register(AttachedBindingMember.CreateMember<ToolStripItem, object>(AttachedMemberConstants.FindByNameMethod,
                    FindByNameMemberToolStripItem));

            //TabControl
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.TabControl.SelectedItem,
                GetSelectedItemTabControl, SetSelectedItemTabControl, "Selected"));

            //ComboBox
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.Control.ItemsSource.Override<ComboBox>(),
                    (info, box) => box.DataSource as IEnumerable,
                    (info, box, value) => box.DataSource = value, "DataSourceChanged"));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.ComboBox.SelectedItem,
                    (info, box) => box.SelectedItem, (info, box, value) => box.SelectedItem = value,
                    "SelectedIndexChanged"));

            //DataGridView
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.Control.ItemsSource.Override<DataGridView>(),
                    (info, view) => view.DataSource as IEnumerable, (info, view, value) =>
                    {
                        view.DataSource = value;
                        view.Refresh();
                    }, "DataSourceChanged"));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.DataGridView.SelectedItem,
                    GetSelectedItemDataGridView, SetSelectedItemDataGridView, (info, view, arg3) =>
                    {
                        arg3 = arg3.ToWeakEventListener();
                        EventHandler handler = null;
                        handler = (sender, args) =>
                        {
                            var gridView = (DataGridView)sender;
                            Action<DataGridView, IEventListener, EventHandler> action =
                                (dataGridView, listener, eventHandler) =>
                                {
                                    if (!listener.TryHandle(dataGridView, EventArgs.Empty))
                                        dataGridView.CurrentCellChanged -= eventHandler;
                                };
                            //To prevent this exception 'Operation not valid because it results in a reentrant call to the SetCurrentCellAddressCore function'
                            gridView.BeginInvoke(action, gridView, arg3, handler);
                        };
                        view.CurrentCellChanged += handler;
                        return WeakActionToken.Create(view, handler,
                            (gridView, eventHandler) => gridView.CurrentCellChanged -= eventHandler);
                    }));
        }

        #region Control

        private static void ContentTemplateChanged(Control control, AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(control, control.GetBindingMemberValue(AttachedMembers.Control.Content), args.NewValue);
        }

        private static void ContentChanged(Control control, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(control, args.NewValue, control.GetBindingMemberValue(AttachedMembers.Control.ContentTemplateSelector));
        }

        private static void UpdateContent(Control container, object value, IDataTemplateSelector selector)
        {
            if (selector != null)
                value = selector.SelectTemplateWithContext(value, container);

            var viewModel = value as IViewModel;
            if (viewModel != null)
                value = ViewModelToViewConverter.Instance.Convert(viewModel);

            var viewManager = container.GetBindingMemberValue(AttachedMembers.Control.ContentViewManager);
            if (viewManager == null)
            {
                container.Controls.Clear();
                var content = value as Control;
                if (content == null && value != null)
                    content = new TextBox
                    {
                        ReadOnly = true,
                        Text = value.ToString(),
                        Multiline = true
                    };
                if (content != null)
                {
                    content.Dock = DockStyle.Fill;
                    content.AutoSize = true;
                    container.Size = content.Size;
                    container.Controls.Add(content);
                }
            }
            else
                viewManager.SetContent(container, value);
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

        private static void ObjectItemsSourceChanged(object control, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var generator = control.GetBindingMemberValue(ItemsSourceGeneratorBase.MemberDescriptor);
            if (generator == null)
            {
                generator = new ItemsSourceGenerator(control);
                control.SetBindingMemberValue(ItemsSourceGeneratorBase.MemberDescriptor, generator);
            }
            generator.SetItemsSource(args.NewValue);
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
                ownerRef = ServiceProvider.WeakReferenceFactory(owner);
            }
            toolStripItem.OwnerChanged += handler;
            var menuItemRef = ServiceProvider.WeakReferenceFactory(toolStripItem);
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
            return tabControl.TabPages[tabControl.SelectedIndex].GetDataContext();
        }

        private static void SetSelectedItemTabControl(IBindingMemberInfo bindingMemberInfo, TabControl tabControl, object item)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (Equals(tabPage.GetDataContext(), item))
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
        protected override IBindingErrorProvider GetBindingErrorProvider(IModuleContext context)
        {
            if (context.Platform.Platform == PlatformType.WinForms)
                return new BindingErrorProvider();
            return null;
        }

        #endregion
    }
}