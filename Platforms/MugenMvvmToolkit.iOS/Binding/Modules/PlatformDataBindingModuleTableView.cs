#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModuleTableView.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.iOS.Binding.Infrastructure;
using MugenMvvmToolkit.iOS.Views;
using MugenMvvmToolkit.Models;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Methods

        private static void RegisterTableViewMembers(IBindingMemberProvider memberProvider)
        {
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewRowAnimation));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewScrollPosition));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellEditingStyle));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellAccessory));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellSelectionStyle));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellSeparatorStyle));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellStyle));

            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.UseAnimations] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.AddAnimation] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.RemoveAnimation] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.ReplaceAnimation] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.ScrollPosition] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.CellBind] = BindingServiceProvider.TemplateMemberPriority + 1;

            //UITableView
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIView.ItemsSource.Override<UITableView>());
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.ReadOnly));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.CellBind));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.AddAnimation));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.RemoveAnimation));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.ReplaceAnimation));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.ScrollPosition));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.CellStyle));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.UseAnimations));
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITableView.SelectedItemChangedEvent));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UITableView>(), TableViewItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UITableView.SelectedItem,
                    GetTableViewSelectedItem, SetTableViewSelectedItem, (info, view, arg3) => (IDisposable)view.SetBindingMemberValue(AttachedMembers.UITableView.SelectedItemChangedEvent, arg3)));
            var itemTemplateMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.ItemTemplateSelector);
            memberProvider.Register(itemTemplateMember);
            memberProvider.Register(typeof(UITableView), AttachedMemberConstants.ItemTemplate, itemTemplateMember, true);

            //UITableViewCell
            BindingBuilderExtensions.RegisterDefaultBindingMember<UITableViewCell>(() => c => c.TextLabel.Text);
            var member = AttachedBindingMember.CreateEvent(AttachedMembers.UITableViewCell.AccessoryButtonTappedEvent);
            memberProvider.Register(member);
            memberProvider.Register("Click", member);
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITableViewCell.DeleteClickEvent));
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITableViewCell.InsertClickEvent));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableViewCell.Moveable));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableViewCell.TitleForDeleteConfirmation));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableViewCell.EditingStyle));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableViewCell.ShouldHighlight));
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITableViewCell.ClickEvent));
            memberProvider.Register(AttachedBindingMember.CreateNotifiableMember(
                AttachedMembers.UITableViewCell.Selected, (info, cell) =>
                {
                    if (TableViewSourceBase.HasMask(cell, TableViewSourceBase.InitializingStateMask))
                        return null;
                    var cellBindable = cell as UITableViewCellBindable;
                    if (cellBindable == null)
                        return cell.Selected;
                    return cellBindable.SelectedBind.GetValueOrDefault();
                }, (info, cell, arg3) =>
                {
                    var cellBindable = cell as UITableViewCellBindable;
                    if (cellBindable == null)
                        cell.Selected = arg3.GetValueOrDefault();
                    else
                        cellBindable.SelectedBind = arg3;
                    return true;
                }));
            memberProvider.Register(AttachedBindingMember.CreateNotifiableMember(
                AttachedMembers.UITableViewCell.Highlighted, (info, cell) => cell.Highlighted,
                (info, cell, arg3) =>
                {
                    if (cell.Highlighted == arg3)
                        return false;
                    cell.Highlighted = arg3;
                    return true;
                }));
            memberProvider.Register(AttachedBindingMember.CreateNotifiableMember(
                AttachedMembers.UITableViewCell.Editing, (info, cell) => cell.Editing,
                (info, cell, arg3) =>
                {
                    if (cell.Editing == arg3)
                        return false;
                    cell.Editing = arg3;
                    return true;
                }));
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
