using System;
using System.Collections;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace

namespace MugenMvvmToolkit
{
    public partial class PlatformDataBindingModule
    {
        #region Fields


        public static readonly INotifiableAttachedBindingMemberInfo<UITableView, object> TableViewSelectedItemChangedEvent;

        public static readonly IAttachedBindingMemberInfo<UITableView, bool> TableViewReadOnlyMember;
        public static readonly IAttachedBindingMemberInfo<UITableView, bool?> TableViewUseAnimationsMember;
        public static readonly IAttachedBindingMemberInfo<UITableView, Action<UITableViewCell>> TableViewCellBindMember;
        public static readonly IAttachedBindingMemberInfo<UITableView, UITableViewCellStyle?> TableViewDefaultCellStyleMember;
        public static readonly IAttachedBindingMemberInfo<UITableView, UITableViewRowAnimation?> TableViewAddAnimationMember;
        public static readonly IAttachedBindingMemberInfo<UITableView, UITableViewRowAnimation?> TableViewRemoveAnimationMember;
        public static readonly IAttachedBindingMemberInfo<UITableView, UITableViewRowAnimation?> TableViewReplaceAnimationMember;
        public static readonly IAttachedBindingMemberInfo<UITableView, UITableViewScrollPosition?> TableViewScrollPositionMember;

        public static readonly INotifiableAttachedBindingMemberInfo<UITableViewCell, object> TableViewCellAccessoryButtonTappedEvent;
        public static readonly INotifiableAttachedBindingMemberInfo<UITableViewCell, object> TableViewCellDeleteClickEvent;
        public static readonly INotifiableAttachedBindingMemberInfo<UITableViewCell, object> TableViewCellInsertClickEvent;
        public static readonly INotifiableAttachedBindingMemberInfo<UITableViewCell, bool> TableViewCellSelectedMember;
        public static readonly INotifiableAttachedBindingMemberInfo<UITableViewCell, bool> TableViewCellHighlightedMember;
        public static readonly INotifiableAttachedBindingMemberInfo<UITableViewCell, bool> TableViewCellEditingMember;

        public static readonly IAttachedBindingMemberInfo<UITableViewCell, bool?> TableViewCellShouldHighlightMember;
        public static readonly IAttachedBindingMemberInfo<UITableViewCell, bool?> TableViewCellMoveableMember;
        public static readonly IAttachedBindingMemberInfo<UITableViewCell, string> TitleForDeleteConfirmationMember;
        public static readonly IAttachedBindingMemberInfo<UITableViewCell, UITableViewCellEditingStyle?> TableViewCellEditingStyleMember;

        #endregion

        #region Methods

        private static void RegisterTableViewMembers(IBindingMemberProvider memberProvider)
        {
            BindingServiceProvider.ResourceResolver.AddType("UITableViewRowAnimation", typeof(UITableViewRowAnimation));
            BindingServiceProvider.ResourceResolver.AddType("UITableViewScrollPosition", typeof(UITableViewScrollPosition));
            BindingServiceProvider.ResourceResolver.AddType("UITableViewCellEditingStyle", typeof(UITableViewCellEditingStyle));
            BindingServiceProvider.ResourceResolver.AddType("UITableViewCellAccessory", typeof(UITableViewCellAccessory));
            BindingServiceProvider.ResourceResolver.AddType("UITableViewCellSelectionStyle", typeof(UITableViewCellSelectionStyle));
            BindingServiceProvider.ResourceResolver.AddType("UITableViewCellSeparatorStyle", typeof(UITableViewCellSeparatorStyle));
            BindingServiceProvider.ResourceResolver.AddType("UITableViewCellStyle", typeof(UITableViewCellStyle));

            BindingServiceProvider.BindingMemberPriorities[TableViewUseAnimationsMember.Path] = 1;
            BindingServiceProvider.BindingMemberPriorities[TableViewAddAnimationMember.Path] = 1;
            BindingServiceProvider.BindingMemberPriorities[TableViewRemoveAnimationMember.Path] = 1;
            BindingServiceProvider.BindingMemberPriorities[TableViewReplaceAnimationMember.Path] = 1;
            BindingServiceProvider.BindingMemberPriorities[TableViewScrollPositionMember.Path] = 1;
            BindingServiceProvider.BindingMemberPriorities[TableViewCellBindMember.Path] = 1;

            //UITableView
            memberProvider.Register(TableViewReadOnlyMember);
            memberProvider.Register(TableViewCellBindMember);
            memberProvider.Register(TableViewAddAnimationMember);
            memberProvider.Register(TableViewRemoveAnimationMember);
            memberProvider.Register(TableViewReplaceAnimationMember);
            memberProvider.Register(TableViewScrollPositionMember);
            memberProvider.Register(TableViewDefaultCellStyleMember);
            memberProvider.Register(TableViewSelectedItemChangedEvent);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UITableView, IEnumerable>(AttachedMemberConstants.ItemsSource, TableViewItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateMember<UITableView, object>(AttachedMemberConstants.SelectedItem,
                    GetTableViewSelectedItem, SetTableViewSelectedItem, (info, view, arg3) => (IDisposable)TableViewSelectedItemChangedEvent.SetValue(view, arg3)));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UITableView, ITableCellTemplateSelector>(AttachedMemberConstants.ItemTemplate));

            //UITableViewCell
            memberProvider.Register(TableViewCellAccessoryButtonTappedEvent);
            memberProvider.Register("Click", TableViewCellAccessoryButtonTappedEvent);
            memberProvider.Register(TableViewCellDeleteClickEvent);
            memberProvider.Register(TableViewCellInsertClickEvent);
            memberProvider.Register(TableViewCellMoveableMember);
            memberProvider.Register(TitleForDeleteConfirmationMember);
            memberProvider.Register(TableViewCellEditingStyleMember);
            memberProvider.Register(TableViewCellSelectedMember);
            memberProvider.Register(TableViewCellHighlightedMember);
            memberProvider.Register(TableViewCellEditingMember);
            memberProvider.Register(TableViewCellShouldHighlightMember);
        }

        private static void SetTableViewSelectedItem(IBindingMemberInfo bindingMemberInfo, UITableView uiTableView, object arg3)
        {
            var tableViewSource = uiTableView.Source as TableViewSourceBase;
            if (tableViewSource != null)
                tableViewSource.SelectedItem = arg3;
        }

        private static object GetTableViewSelectedItem(IBindingMemberInfo bindingMemberInfo, UITableView uiTableView)
        {
            var tableViewSource = uiTableView.Source as TableViewSourceBase;
            if (tableViewSource == null)
                return null;
            return tableViewSource.SelectedItem;
        }

        private static void TableViewItemsSourceChanged(UITableView uiTableView, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            if (uiTableView.Source == null)
                uiTableView.Source = TableViewSourceBase.Factory(uiTableView, DataContext.Empty);
            var tableViewSource = uiTableView.Source as ItemsSourceTableViewSource;
            if (tableViewSource != null)
                tableViewSource.ItemsSource = args.NewValue;
        }

        #endregion
    }
}