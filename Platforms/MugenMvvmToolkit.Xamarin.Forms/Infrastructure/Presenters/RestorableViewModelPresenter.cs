#region Copyright

// ****************************************************************************
// <copyright file="RestorableViewModelPresenter.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Presenters;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Presenters
{
    public class RestorableViewModelPresenter : ViewModelPresenter, IRestorableViewModelPresenter
    {
        #region Nested types

        private sealed class ViewModelState
        {
            #region Fields

            public readonly string Id;
            public int Index;
            public byte[] State;

            #endregion

            #region Constructors

            public ViewModelState(string id)
            {
                Id = id;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly List<IViewModel> _openedViewModels;
        private readonly ISerializer _serializer;
        private readonly IViewModelProvider _viewModelProvider;

        private const string StatePrefix = "vmst_";
        private const string NumberPrefix = "vmnum_";

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public RestorableViewModelPresenter(INavigationDispatcher navigationDispatcher, IViewModelProvider viewModelProvider, ISerializer serializer)
            : base(navigationDispatcher)
        {
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            _viewModelProvider = viewModelProvider;
            _serializer = serializer;
            _openedViewModels = new List<IViewModel>();
            GetStateDictionary = () => Application.Current.Properties;
        }

        #endregion

        #region Properties

        public Func<IDictionary<string, object>> GetStateDictionary { get; set; }

        #endregion

        #region Methods

        protected override IAsyncOperation ShowInternalAsync(IDataContext context)
        {
            var result = base.ShowInternalAsync(context);
            var viewModel = context.GetData(NavigationConstants.ViewModel) ?? result.Context.GetData(NavigationConstants.ViewModel);
            if (viewModel != null)
            {
                _openedViewModels.Add(viewModel);
                result.ContinueWith(OnViewModelClosed);
            }
            return result;
        }

        protected virtual void SaveStateInternal([NotNull] IDataContext context)
        {
            var dictionary = GetStateDictionary?.Invoke();
            var viewModels = _openedViewModels.ToArray();
            if (viewModels.Length == 0 || dictionary == null)
                return;
            for (var i = 0; i < viewModels.Length; i++)
            {
                var viewModel = viewModels[i];
                var vmId = viewModel.GetViewModelId().ToString("n");
                var state = _serializer.Serialize(_viewModelProvider.PreserveViewModel(viewModel, context)).ToArray();
                dictionary[NumberPrefix + vmId] = i;
                dictionary[StatePrefix + vmId] = state;
            }
        }

        protected virtual void ClearStateInternal(IDataContext context)
        {
            IDictionary<string, object> dictionary;
            var items = GetItems(out dictionary);
            if (items == null || items.Count == 0)
                return;
            foreach (ViewModelState item in items)
            {
                dictionary.Remove(NumberPrefix + item.Id);
                dictionary.Remove(StatePrefix + item.Id);
            }
        }

        protected virtual bool TryRestoreInternal(IDataContext context)
        {
            IDictionary<string, object> dictionary;
            var items = GetItems(out dictionary);
            if (items == null || items.Count == 0)
                return false;
            foreach (var item in items.OrderBy(tuple => tuple.Index))
            {
                dictionary.Remove(NumberPrefix + item.Id);
                dictionary.Remove(StatePrefix + item.Id);
                if (item.State != null)
                {
                    using (var ms = new MemoryStream(item.State))
                    {
                        var dataContext = (IDataContext)_serializer.Deserialize(ms);
                        var viewModel = _viewModelProvider.RestoreViewModel(dataContext, context, true);
                        viewModel.ShowAsync(context);
                    }
                }
            }
            return true;
        }

        private void OnViewModelClosed(IOperationResult operationResult)
        {
            var viewModel = operationResult.Source as IViewModel;
            if (viewModel == null)
                return;
            _openedViewModels.Remove(viewModel);
            var dictionary = GetStateDictionary?.Invoke();
            if (dictionary == null)
                return;
            var vmId = viewModel.GetViewModelId().ToString("n");
            dictionary.Remove(NumberPrefix + vmId);
            dictionary.Remove(StatePrefix + vmId);
        }

        private List<ViewModelState> GetItems(out IDictionary<string, object> dictionary)
        {
            dictionary = GetStateDictionary?.Invoke();
            if (dictionary == null)
                return null;
            var items = new List<ViewModelState>();
            foreach (var keyPair in dictionary)
            {
                if (keyPair.Key.StartsWith(StatePrefix, StringComparison.Ordinal))
                {
                    var id = keyPair.Key.Replace(StatePrefix, string.Empty);
                    var restoreTuple = items.Find(tuple => tuple.Id == id);
                    if (restoreTuple == null)
                    {
                        restoreTuple = new ViewModelState(id);
                        items.Add(restoreTuple);
                    }
                    restoreTuple.State = (byte[])keyPair.Value;
                }
                else if (keyPair.Key.StartsWith(NumberPrefix, StringComparison.Ordinal))
                {
                    var id = keyPair.Key.Replace(StatePrefix, string.Empty);
                    var restoreTuple = items.Find(tuple => tuple.Id == id);
                    if (restoreTuple == null)
                    {
                        restoreTuple = new ViewModelState(id);
                        items.Add(restoreTuple);
                    }
                    restoreTuple.Index = (int)keyPair.Value;
                }
            }
            return items;
        }

        #endregion

        #region Implementation of interfaces

        public void SaveState(IDataContext context)
        {
            SaveStateInternal(context ?? DataContext.Empty);
        }

        public void ClearState(IDataContext context)
        {
            ClearStateInternal(context ?? DataContext.Empty);
        }

        public bool TryRestore(IDataContext context)
        {
            bool data;
            if (context != null && context.TryGetData(ViewModelConstants.StateNotNeeded, out data) && data)
                return false;
            return TryRestoreInternal(context ?? DataContext.Empty);
        }

        #endregion
    }
}