#region Copyright

// ****************************************************************************
// <copyright file="ViewModelPresenter.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Represents the service that allows to show a view model.
    /// </summary>
    public class ViewModelPresenter : IViewModelPresenter
    {
        #region Nested types

        private sealed class DynamicPresentersCollection : ICollection<IDynamicViewModelPresenter>
        {
            #region Fields

            private readonly ViewModelPresenter _presenter;
            private readonly OrderedListInternal<IDynamicViewModelPresenter> _list;

            #endregion

            #region Constructors

            public DynamicPresentersCollection(ViewModelPresenter presenter)
            {
                _presenter = presenter;
                _list = new OrderedListInternal<IDynamicViewModelPresenter>(new DelegateComparer<IDynamicViewModelPresenter>(ComparerDelegate));
            }

            #endregion

            #region Methods

            public IDynamicViewModelPresenter this[int index]
            {
                get { return _list[index]; }
            }

            private static int ComparerDelegate(IDynamicViewModelPresenter x1, IDynamicViewModelPresenter x2)
            {
                return x2.Priority.CompareTo(x1.Priority);
            }

            #endregion

            #region Implementation of ICollection<IDynamicViewModelPresenter>

            public IEnumerator<IDynamicViewModelPresenter> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(IDynamicViewModelPresenter item)
            {
                Should.NotBeNull(item, "item");
                _list.Add(item);
                _presenter.OnDynamicPresenterAdded(item);
            }

            public void Clear()
            {
                var values = _list.ToArrayEx();
                _list.Clear();
                for (int index = 0; index < values.Length; index++)
                    _presenter.OnDynamicPresenterRemoved(values[index]);
            }

            public bool Contains(IDynamicViewModelPresenter item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(IDynamicViewModelPresenter[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(IDynamicViewModelPresenter item)
            {
                Should.NotBeNull(item, "item");
                var remove = _list.Remove(item);
                if (remove)
                    _presenter.OnDynamicPresenterRemoved(item);
                return remove;
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Gets the default navigtion presenter priority.
        /// </summary>
        public const int DefaultNavigationPresenterPriority = -1;

        /// <summary>
        ///     Gets the default view model presenter priority.
        /// </summary>
        public const int DefaultMultiViewModelPresenterPriority = 0;

        /// <summary>
        ///     Gets the default window presenter priority.
        /// </summary>
        public const int DefaultWindowPresenterPriority = 1;

        private readonly DynamicPresentersCollection _dynamicPresenters;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelPresenter" /> class.
        /// </summary>
        public ViewModelPresenter()
        {
            _dynamicPresenters = new DynamicPresentersCollection(this);
        }

        #endregion

        #region Implementation of IViewModelPresenter

        /// <summary>
        ///     Gets the collection of <see cref="IDynamicViewModelPresenter" />.
        /// </summary>
        public ICollection<IDynamicViewModelPresenter> DynamicPresenters
        {
            get { return _dynamicPresenters; }
        }

        /// <summary>
        ///     Shows the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        public virtual INavigationOperation ShowAsync(IViewModel viewModel, IDataContext context)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = DataContext.Empty;
            for (int i = 0; i < _dynamicPresenters.Count; i++)
            {
                var operation = _dynamicPresenters[i].TryShowAsync(viewModel, context, this);
                if (operation != null)
                    return operation;
            }
            throw ExceptionManager.PresenterCannotShowViewModel(GetType(), viewModel.GetType());
        }

        /// <summary>
        ///     Tries to restore the presenter state of the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        public virtual void Restore(IViewModel viewModel, IDataContext context)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = DataContext.Empty;
            for (int i = 0; i < _dynamicPresenters.Count; i++)
            {
                var presenter = _dynamicPresenters[i] as IRestorableDynamicViewModelPresenter;
                if (presenter != null && presenter.Restore(viewModel, context, this))
                    return;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Occurs when presenter added.
        /// </summary>
        protected virtual void OnDynamicPresenterAdded([NotNull] IDynamicViewModelPresenter presenter)
        {
        }

        /// <summary>
        ///     Occurs when presenter removed.
        /// </summary>
        protected virtual void OnDynamicPresenterRemoved([NotNull] IDynamicViewModelPresenter presenter)
        {
        }

        #endregion
    }
}