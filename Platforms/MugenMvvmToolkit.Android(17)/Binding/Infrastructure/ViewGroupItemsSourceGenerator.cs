#region Copyright
// ****************************************************************************
// <copyright file="ViewGroupItemsSourceGenerator.cs">
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

using System.Collections;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    internal sealed class ViewGroupItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        internal static readonly DataContext Context;
        private readonly IItemsSourceAdapter _adapter;
        private readonly ViewGroup _viewGroup;

        #endregion

        #region Constructors

        static ViewGroupItemsSourceGenerator()
        {
            Context = new DataContext();
        }

        private ViewGroupItemsSourceGenerator([NotNull] ViewGroup viewGroup)
        {
            Should.NotBeNull(viewGroup, "viewGroup");
            _viewGroup = viewGroup;
            _adapter = ItemsSourceAdapter.Factory(viewGroup, viewGroup.Context, Context);
            TryListenActivity(viewGroup.Context);
        }

        #endregion

        #region Methods

        public static IItemsSourceGenerator GetOrAdd(ViewGroup viewGroup)
        {
            return ServiceProvider.AttachedValueProvider.GetOrAdd(viewGroup, Key,
                (@group, o) => new ViewGroupItemsSourceGenerator(@group), null);
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource
        {
            get { return _adapter.ItemsSource; }
            set { _adapter.ItemsSource = value; }
        }

        protected override void Add(int insertionIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                _viewGroup.AddView(_adapter.GetView(index, null, _viewGroup), index);
            }
        }

        protected override void Remove(int removalIndex, int count)
        {
            for (int i = 0; i < count; i++)
                _viewGroup.RemoveViewAt(removalIndex + i);
        }

        protected override void Replace(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                _viewGroup.RemoveViewAt(index);
                _viewGroup.AddView(_adapter.GetView(index, null, _viewGroup), index);
            }
        }

        protected override void Refresh()
        {
            _viewGroup.RemoveAllViews();
            int count = _adapter.Count;
            for (int i = 0; i < count; i++)
                _viewGroup.AddView(_adapter.GetView(i, null, _viewGroup));
        }

        #endregion
    }
}