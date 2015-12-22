#region Copyright

// ****************************************************************************
// <copyright file="ViewGroupItemsSourceGenerator.cs">
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

using System.Collections;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    internal sealed class ViewGroupItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        internal static readonly DataContext Context;
        private readonly IItemsSourceAdapter _adapter;
        private readonly IBindingMemberInfo _collectionViewManagerMember;
        private readonly ViewGroup _viewGroup;

        #endregion

        #region Constructors

        static ViewGroupItemsSourceGenerator()
        {
            Context = new DataContext();
        }

        internal ViewGroupItemsSourceGenerator([NotNull] ViewGroup viewGroup)
        {
            Should.NotBeNull(viewGroup, nameof(viewGroup));
            _viewGroup = viewGroup;
            _adapter = ItemsSourceAdapter.Factory(viewGroup, viewGroup.Context, Context);
            _collectionViewManagerMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(viewGroup.GetType(), AttachedMembers.ViewGroup.CollectionViewManager, false, false);
            TryListenActivity(viewGroup.Context);
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource
        {
            get { return _adapter.ItemsSource; }
            set { _adapter.ItemsSource = value; }
        }

        protected override bool IsTargetDisposed => !_viewGroup.IsAlive();

        protected override void Add(int insertionIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                Add(_adapter.GetView(index, null, _viewGroup), index);
            }
        }

        protected override void Remove(int removalIndex, int count)
        {
            for (int i = 0; i < count; i++)
                RemoveAt(removalIndex + i);
        }

        protected override void Replace(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                RemoveAt(index);
                Add(_adapter.GetView(index, null, _viewGroup), index);
            }
        }

        protected override void Refresh()
        {
            Clear();
            int count = _adapter.Count;
            for (int i = 0; i < count; i++)
                Add(_adapter.GetView(i, null, _viewGroup), i);
        }

        #endregion

        #region Methods

        private ICollectionViewManager GetCollectionViewManager()
        {
            return _collectionViewManagerMember == null
                ? null
                : _collectionViewManagerMember.GetValue(_viewGroup, null) as ICollectionViewManager;
        }

        private void Add(View view, int index)
        {
            var collectionViewManager = GetCollectionViewManager();
            if (collectionViewManager == null)
                _viewGroup.AddView(view, index);
            else
                collectionViewManager.Insert(_viewGroup, index, view);
        }

        private void RemoveAt(int index)
        {
            var collectionViewManager = GetCollectionViewManager();
            if (collectionViewManager == null)
                _viewGroup.RemoveViewAt(index);
            else
                collectionViewManager.RemoveAt(_viewGroup, index);
        }

        private void Clear()
        {
            var collectionViewManager = GetCollectionViewManager();
            if (collectionViewManager == null)
            {
                while (_viewGroup.ChildCount != 0)
                    RemoveAt(0);
            }
            else
                collectionViewManager.Clear(_viewGroup);
        }

        #endregion
    }
}
