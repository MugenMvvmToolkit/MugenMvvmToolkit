#region Copyright

// ****************************************************************************
// <copyright file="RestorableViewModelPresenter.cs">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
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

        private readonly HashSet<IViewModel> _openedViewModels;
        private readonly ISerializer _serializer;
        private readonly IViewModelProvider _viewModelProvider;

        private const string StatePrefix = "vmst_";
        private const string NumberPrefix = "vmnum_";

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public RestorableViewModelPresenter(IViewModelProvider viewModelProvider, ISerializer serializer)
        {
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            _viewModelProvider = viewModelProvider;
            _serializer = serializer;
            _openedViewModels = new HashSet<IViewModel>(ReferenceEqualityComparer.Instance);
            GetStateDictionary = () => Application.Current.Properties;
        }

        #endregion

        #region Properties

        public Func<IDictionary<string, object>> GetStateDictionary { get; set; }

        #endregion

        #region Methods

        private void OnViewModelClosed(IOperationResult<bool> operationResult)
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

        public virtual void SaveState(IDataContext context)
        {
            var dictionary = GetStateDictionary?.Invoke();
            var viewModels = _openedViewModels.ToArray();
            if ((viewModels.Length == 0) || (dictionary == null))
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

        public virtual void ClearState(IDataContext context = null)
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

        public virtual bool TryRestore(IDataContext context = null)
        {
            IDictionary<string, object> dictionary;
            var items = GetItems(out dictionary);
            if (items == null || items.Count == 0)
                return false;
            var viewModels = new List<IViewModel>();
            foreach (var item in items.OrderBy(tuple => tuple.Index))
            {
                dictionary.Remove(NumberPrefix + item.Id);
                dictionary.Remove(StatePrefix + item.Id);
                if (item.State != null)
                    using (var ms = new MemoryStream(item.State))
                    {
                        var dataContext = (IDataContext)_serializer.Deserialize(ms);
                        viewModels.Add(_viewModelProvider.RestoreViewModel(dataContext, context, true));
                    }
            }
            for (var i = 0; i < viewModels.Count; i++)
                viewModels[i].ShowAsync(context);
            return true;
        }

        public override INavigationOperation ShowAsync(IViewModel viewModel, IDataContext context)
        {
            var result = base.ShowAsync(viewModel, context);
            _openedViewModels.Add(viewModel);
            result.ContinueWith(OnViewModelClosed);
            return result;
        }

        #endregion
    }
}