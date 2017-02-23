#region Copyright

// ****************************************************************************
// <copyright file="ViewModelPresenter.cs">
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

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
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

            public IDynamicViewModelPresenter this[int index] => _list[index];

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
                Should.NotBeNull(item, nameof(item));
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
                Should.NotBeNull(item, nameof(item));
                var remove = _list.Remove(item);
                if (remove)
                    _presenter.OnDynamicPresenterRemoved(item);
                return remove;
            }

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            #endregion
        }

        #endregion

        #region Fields

        public const int DefaultNavigationPresenterPriority = -1;
        public const int DefaultMultiViewModelPresenterPriority = 0;
        public const int DefaultWindowPresenterPriority = 1;

        private readonly INavigationDispatcher _navigationDispatcher;
        private readonly DynamicPresentersCollection _dynamicPresenters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(INavigationDispatcher navigationDispatcher)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            _navigationDispatcher = navigationDispatcher;
            _dynamicPresenters = new DynamicPresentersCollection(this);
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher;

        #endregion

        #region Implementation of IViewModelPresenter

        public ICollection<IDynamicViewModelPresenter> DynamicPresenters => _dynamicPresenters;

        public IAsyncOperation ShowAsync(IViewModel viewModel, IDataContext context)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return ShowInternalAsync(viewModel, context ?? DataContext.Empty);
        }

        public void Restore(IViewModel viewModel, IDataContext context)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            RestoreInternal(viewModel, context ?? DataContext.Empty);
        }

        public Task<bool> CloseAsync(IViewModel viewModel, IDataContext context)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return CloseInternalAsync(viewModel, context ?? DataContext.Empty);
        }

        #endregion

        #region Methods

        protected virtual IAsyncOperation ShowInternalAsync(IViewModel viewModel, IDataContext context)
        {
            var presenters = _dynamicPresenters.ToArrayEx();
            for (int i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryShowAsync(viewModel, context, this);
                if (operation != null)
                {
                    if (Tracer.TraceInformation)
                        Tracer.Info("The {0} is shown by {1}", viewModel.GetType().FullName, presenters[i].GetType().FullName);
                    return operation;
                }
            }
            throw ExceptionManager.PresenterCannotShowViewModel(GetType(), viewModel.GetType());
        }

        protected virtual void RestoreInternal(IViewModel viewModel, IDataContext context)
        {
            var presenters = _dynamicPresenters.ToArrayEx();
            for (int i = 0; i < presenters.Length; i++)
            {
                var presenter = presenters[i] as IRestorableDynamicViewModelPresenter;
                if (presenter != null && presenter.Restore(viewModel, context, this))
                    return;
            }
        }

        [NotNull]
        protected virtual Task<bool> CloseInternalAsync(IViewModel viewModel, IDataContext context)
        {
            var presenters = _dynamicPresenters.ToArrayEx();
            for (int i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryCloseAsync(viewModel, context, this);
                if (operation != null)
                {
                    if (Tracer.TraceInformation)
                        Tracer.Info("The {0} is closed by {1}", viewModel.GetType().FullName, presenters[i].GetType().FullName);
                    return operation;
                }
            }
            var wrapperViewModel = viewModel.Settings.Metadata.GetData(ViewModelConstants.WrapperViewModel);
            if (wrapperViewModel != null)
                return CloseInternalAsync(wrapperViewModel, context);
            var navigationContext = context as INavigationContext;
            if (navigationContext == null)
                return Empty.FalseTask;
            return NavigationDispatcher.NavigatingFromAsync(navigationContext);
        }

        protected virtual void OnDynamicPresenterAdded([NotNull] IDynamicViewModelPresenter presenter)
        {
        }

        protected virtual void OnDynamicPresenterRemoved([NotNull] IDynamicViewModelPresenter presenter)
        {
        }

        #endregion
    }
}
