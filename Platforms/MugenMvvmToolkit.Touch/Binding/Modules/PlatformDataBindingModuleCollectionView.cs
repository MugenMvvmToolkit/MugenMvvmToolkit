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
using UIKit;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Fields

        public static readonly INotifiableAttachedBindingMemberInfo<UICollectionView, object> CollectionViewSelectedItemChangedEvent;
        public static readonly IAttachedBindingMemberInfo<UICollectionView, bool?> CollectionViewUseAnimationsMember;
        public static readonly IAttachedBindingMemberInfo<UICollectionView, UICollectionViewScrollPosition?> CollectionViewScrollPositionMember;

        public static readonly INotifiableAttachedBindingMemberInfo<UICollectionViewCell, bool> CollectionViewCellSelectedMember;
        public static readonly INotifiableAttachedBindingMemberInfo<UICollectionViewCell, bool> CollectionViewCellHighlightedMember;

        public static readonly IAttachedBindingMemberInfo<UICollectionViewCell, bool?> CollectionViewCellShouldHighlightMember;
        public static readonly IAttachedBindingMemberInfo<UICollectionViewCell, bool?> CollectionViewCellShouldDeselectMember;
        public static readonly IAttachedBindingMemberInfo<UICollectionViewCell, bool?> CollectionViewCellShouldSelectMember;

        #endregion

        #region Methods

        private static void RegisterCollectionViewMembers(IBindingMemberProvider memberProvider)
        {
            BindingServiceProvider.ResourceResolver.AddType("UICollectionViewScrollPosition", typeof(UICollectionViewScrollPosition));
            BindingServiceProvider.ResourceResolver.AddType("UICollectionViewScrollDirection", typeof(UICollectionViewScrollDirection));

            BindingServiceProvider.BindingMemberPriorities[CollectionViewUseAnimationsMember.Path] = 1;
            BindingServiceProvider.BindingMemberPriorities[CollectionViewScrollPositionMember.Path] = 1;

            //UICollectionView
            memberProvider.Register(CollectionViewUseAnimationsMember);
            memberProvider.Register(CollectionViewScrollPositionMember);
            memberProvider.Register(CollectionViewSelectedItemChangedEvent);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UICollectionView, IEnumerable>(AttachedMemberConstants.ItemsSource, CollectionViewItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateMember<UICollectionView, object>(AttachedMemberConstants.SelectedItem,
                    GetCollectionViewSelectedItem, SetCollectionViewSelectedItem, (info, view, arg3) => (IDisposable)CollectionViewSelectedItemChangedEvent.SetValue(view, arg3)));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UICollectionView, ICollectionCellTemplateSelector>(
                    AttachedMemberConstants.ItemTemplate,
                    (view, args) =>
                    {
                        if (args.NewValue != null)
                            args.NewValue.Initialize(view);
                    }));

            //UICollectionViewCell
            memberProvider.Register(CollectionViewCellSelectedMember);
            memberProvider.Register(CollectionViewCellHighlightedMember);
            memberProvider.Register(CollectionViewCellShouldHighlightMember);
            memberProvider.Register(CollectionViewCellShouldDeselectMember);
            memberProvider.Register(CollectionViewCellShouldSelectMember);
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