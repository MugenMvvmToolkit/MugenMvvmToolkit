using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.WinForms.Binding.Converters;
using MugenMvvmToolkit.WinForms.Binding.Infrastructure;
using MugenMvvmToolkit.WinForms.Binding.Interfaces;

namespace MugenMvvmToolkit.WinForms.Binding
{
    public static class AttachedMembersRegistration
    {
        #region Properties

        private static IBindingMemberProvider MemberProvider => BindingServiceProvider.MemberProvider;

        #endregion

        #region Methods

        public static void RegisterObjectMembers()
        {
            //Object
            var itemsSourceMember = AttachedBindingMember.CreateAutoProperty<object, IEnumerable>(AttachedMemberConstants.ItemsSource, ObjectItemsSourceChanged);
            var defaultMemberRegistration = new DefaultAttachedMemberRegistration<IEnumerable>(itemsSourceMember);
            MemberProvider.Register(defaultMemberRegistration.ToAttachedBindingMember<object>());
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Object.CollectionViewManager));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IContentViewManager>(AttachedMembers.Control.ContentViewManager.Path));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(ItemsSourceGeneratorBase.MemberDescriptor,
                (o, args) =>
                {
                    IEnumerable itemsSource = null;
                    if (args.OldValue != null)
                    {
                        itemsSource = args.OldValue.ItemsSource;
                        args.OldValue.SetItemsSource(null);
                    }
                    args.NewValue?.SetItemsSource(itemsSource);
                }));

            var itemTemplateMember = AttachedBindingMember.CreateAutoProperty<object, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
            MemberProvider.Register(itemTemplateMember);
            MemberProvider.Register(typeof(object), AttachedMemberConstants.ItemTemplate, itemTemplateMember, true);
        }

        public static void RegisterButtonMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Button>(nameof(Button.Click));
        }

        public static void RegisterTextBoxMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<TextBox>(nameof(TextBox.Text));
        }

        public static void RegisterLabelMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Label>(nameof(Label.Text));
        }

        public static void RegisterCheckBoxMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<CheckBox>(nameof(CheckBox.Checked));
        }

        public static void RegisterProgressBarMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ProgressBar>(nameof(ProgressBar.Value));
        }

        public static void RegisterFormMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Form.ToastTemplateSelector));
        }

        public static void RegisterControlMembers()
        {
            var sizeChanged = MemberProvider.GetBindingMember(typeof(Control), nameof(Control.SizeChanged), true, false);
            if (sizeChanged != null)
            {
                MemberProvider.Register(typeof(Control), "WidthChanged", sizeChanged, true);
                MemberProvider.Register(typeof(Control), "HeightChanged", sizeChanged, true);
            }
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<Control, object>(AttachedMemberConstants.FindByNameMethod, FindByNameControlMember));
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<Control, bool>(AttachedMemberConstants.Focused, (info, control) => control.Focused,
                    (info, control, arg3) =>
                    {
                        if (arg3)
                            control.Focus();
                    }, nameof(Control.LostFocus)));

            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Control.Content, ContentChanged));
            var contenMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.Control.ContentTemplateSelector, ContentTemplateChanged);
            MemberProvider.Register(contenMember);
            MemberProvider.Register(typeof(Control), AttachedMemberConstants.ContentTemplate, contenMember, true);
        }

        public static void RegisterDateTimePickerMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<DateTimePicker>(nameof(DateTimePicker.Value));
            MemberProvider.Register(AttachedBindingMember.CreateMember<DateTimePicker, DateTime>(nameof(DateTimePicker.Value),
                (info, picker) => picker.Value,
                (info, picker, value) =>
                {
                    if (value < picker.MinDate)
                        picker.Value = picker.MinDate;
                    else if (value > picker.MaxDate)
                        picker.Value = picker.MaxDate;
                    else
                        picker.Value = value;
                }, nameof(DateTimePicker.ValueChanged)));
        }

        public static void RegisterToolStripItemMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ToolStripButton>(nameof(ToolStripButton.Click));
            BindingBuilderExtensions.RegisterDefaultBindingMember<ToolStripMenuItem>(nameof(ToolStripMenuItem.Click));
            BindingBuilderExtensions.RegisterDefaultBindingMember<ToolStripItem>(nameof(ToolStripItem.Text));
            MemberProvider.Register(AttachedBindingMember.CreateMember<ToolStripItem, object>(AttachedMemberConstants.ParentExplicit,
                GetParentToolStripItem, null, ObserveParentMemberToolStripItem));
            MemberProvider.Register(AttachedBindingMember.CreateMember<ToolStripItem, object>(AttachedMemberConstants.FindByNameMethod,
                FindByNameMemberToolStripItem));
        }

        public static void RegisterTabControlMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.Object.ItemsSource.Override<TabControl>());
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.TabControl.SelectedItem,
                GetSelectedItemTabControl, SetSelectedItemTabControl, nameof(TabControl.Selected)));
        }

        public static void RegisterComboBoxMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.Object.ItemsSource.Override<ComboBox>());
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.Object.ItemsSource.Override<ComboBox>(),
                (info, box) => box.DataSource as IEnumerable,
                (info, box, value) => box.DataSource = value, nameof(ComboBox.DataSourceChanged)));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.ComboBox.SelectedItem,
                (info, box) => box.SelectedItem, (info, box, value) => box.SelectedItem = value, nameof(ComboBox.SelectedIndexChanged)));
        }

        public static void RegisterDataGridViewMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.Object.ItemsSource.Override<DataGridView>());
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.Object.ItemsSource.Override<DataGridView>(),
                (info, view) => view.DataSource as IEnumerable, (info, view, value) =>
                {
                    view.DataSource = value;
                    view.Refresh();
                }, nameof(DataGridView.DataSourceChanged)));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.DataGridView.SelectedItem,
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
                if ((content == null) && (value != null))
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
            var owner = GetOwner(toolStripItem);
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
            var owner = menuItem.Owner;
            while (owner is ToolStripDropDownMenu)
                owner = (owner as ToolStripDropDownMenu).OwnerItem?.Owner;
            return owner;
        }

        private static void SetSelectedItemDataGridView(IBindingMemberInfo bindingMemberInfo, DataGridView dataGridView, object item)
        {
            dataGridView.ClearSelection();
            if (item == null)
                return;
            for (var i = 0; i < dataGridView.Rows.Count; i++)
                if (Equals(dataGridView.Rows[i].DataBoundItem, item))
                {
                    var row = dataGridView.Rows[i];
                    row.Selected = true;
                    if (row.Cells.Count > 0)
                        row.Cells[0].Selected = true;
                    break;
                }
        }

        private static object GetSelectedItemDataGridView(IBindingMemberInfo bindingMemberInfo, DataGridView dataGridView)
        {
            return dataGridView.CurrentRow?.DataBoundItem;
        }


        private static object GetSelectedItemTabControl(IBindingMemberInfo bindingMemberInfo, TabControl tabControl)
        {
            if ((tabControl.TabCount == 0) || (tabControl.SelectedIndex < 0))
                return null;
            return tabControl.TabPages[tabControl.SelectedIndex].DataContext();
        }

        private static void SetSelectedItemTabControl(IBindingMemberInfo bindingMemberInfo, TabControl tabControl, object item)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
                if (Equals(tabPage.DataContext(), item))
                {
                    tabControl.SelectedTab = tabPage;
                    break;
                }
        }

        #endregion
    }
}