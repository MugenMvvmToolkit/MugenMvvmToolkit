#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModuleCollectionView.cs">
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

        private static void RegisterCollectionViewMembers(IBindingMemberProvider memberProvider)
        {
            BindingServiceProvider.ResourceResolver.AddType(typeof(UICollectionViewScrollPosition));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UICollectionViewScrollDirection));

            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UICollectionView.UseAnimations] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UICollectionView.ScrollPosition] = BindingServiceProvider.TemplateMemberPriority + 1;

            //UICollectionView
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIView.ItemsSource.Override<UICollectionView>());
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionView.UseAnimations));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionView.ScrollPosition));
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UICollectionView.SelectedItemChangedEvent));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UICollectionView>(), CollectionViewItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UICollectionView.SelectedItem,
                    GetCollectionViewSelectedItem, SetCollectionViewSelectedItem, (info, view, arg3) => (IDisposable)view.SetBindingMemberValue(AttachedMembers.UICollectionView.SelectedItemChangedEvent, arg3)));
            var itemTemplateMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionView.ItemTemplateSelector,
                (view, args) =>
                {
                    if (args.NewValue != null)
                        args.NewValue.Initialize(view);
                });
            memberProvider.Register(itemTemplateMember);
            memberProvider.Register(typeof(UICollectionView), AttachedMemberConstants.ItemTemplate, itemTemplateMember, true);

            //UICollectionViewCell
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.UICollectionViewCell.Selected, (info, cell) =>
                {
                    if (CollectionViewSourceBase.HasMask(cell, CollectionViewSourceBase.InitializingStateMask))
                        return null;
                    var cellBindable = cell as UICollectionViewCellBindable;
                    if (cellBindable == null)
                        return cell.Selected;
                    return cellBindable.SelectedBind.GetValueOrDefault();
                },
                    (info, cell, arg3) =>
                    {
                        var cellBindable = cell as UICollectionViewCellBindable;
                        if (cellBindable == null)
                            cell.Selected = arg3.GetValueOrDefault();
                        else
                            cellBindable.SelectedBind = arg3;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.UICollectionViewCell.Highlighted, (info, cell) => cell.Highlighted,
                    (info, cell, arg3) =>
                    {
                        cell.Highlighted = arg3;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionViewCell.ShouldHighlight));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionViewCell.ShouldDeselect));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionViewCell.ShouldSelect));
        }

        private static void SetCollectionViewSelectedItem(IBindingMemberInfo bindingMemberInfo, UICollectionView collectionView, object arg3)
        {
            var source = collectionView.Source as CollectionViewSourceBase;
            if (source != null)
                source.SelectedItem = arg3;
        }

        private static object GetCollectionViewSelectedItem(IBindingMemberInfo bindingMemberInfo, UICollectionView collectionView)
        {
            var source = collectionView.Source as CollectionViewSourceBase;
            if (source == null)
                return null;
            return source.SelectedItem;
        }

        private static void CollectionViewItemsSourceChanged(UICollectionView collectionView, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            if (collectionView.Source == null)
                collectionView.Source = CollectionViewSourceBase.Factory(collectionView, DataContext.Empty);
            var source = collectionView.Source as ItemsSourceCollectionViewSource;
            if (source != null)
                source.ItemsSource = args.NewValue;
        }

        #endregion
    }
}
