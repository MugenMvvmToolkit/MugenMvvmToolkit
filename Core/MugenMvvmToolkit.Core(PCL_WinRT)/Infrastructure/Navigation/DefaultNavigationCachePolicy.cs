#region Copyright
// ****************************************************************************
// <copyright file="DefaultNavigationCachePolicy.cs">
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
using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    /// <summary>
    ///     Represents the view model navigation cache policy, that clear the cache during back navigation.
    /// </summary>
    public class DefaultNavigationCachePolicy : INavigationCachePolicy
    {
        #region Fields

        private readonly Dictionary<Type, List<IViewModel>> _cachedViewModels;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultNavigationCachePolicy" /> class.
        /// </summary>
        public DefaultNavigationCachePolicy()
        {
            _cachedViewModels = new Dictionary<Type, List<IViewModel>>();
        }

        #endregion

        #region Implementation of INavigationCachePolicy

        /// <summary>
        ///     Tries to save a view model in the cache.
        /// </summary>
        public virtual void TryCacheViewModel(INavigationContext context, object view, IViewModel viewModel)
        {
            if (context.NavigationMode == NavigationMode.Back)
                return;
            view = GetView(view);
            Type type = view.GetType();

            List<IViewModel> list;
            if (!_cachedViewModels.TryGetValue(type, out list))
            {
                list = new List<IViewModel>();
                _cachedViewModels[type] = list;
            }
            list.Insert(0, viewModel);
            Tracer.Info("Navigation cache - the view model {0} was cached, navigation mode: {1}, view: {2}",
                viewModel.GetType(), context.NavigationMode, type);
        }

        /// <summary>
        ///     Tries to get view model from the cache, and delete it from the cache.
        /// </summary>
        public virtual IViewModel TryTakeViewModelFromCache(INavigationContext context, object view)
        {
            view = GetView(view);
            var type = view.GetType();
            List<IViewModel> list;
            if (!_cachedViewModels.TryGetValue(type, out list) || list == null || list.Count == 0)
            {
                Tracer.Info("Navigation cache - the view model for the view {0} is not found in the cache, navigation mode: {1}", type, context.NavigationMode);
                return null;
            }
            IViewModel vm = list[0];
            list.RemoveAt(0);
            Tracer.Info("Navigation cache - the view model {0} for the view {1} was taken from the cache, navigation mode: {2}",
                vm.GetType(), type, context.NavigationMode);
            return vm;
        }

        /// <summary>
        ///     Gets the cached view models.
        /// </summary>
        public virtual IList<IViewModel> GetViewModels(IDataContext context)
        {
            var list = new List<IViewModel>();
            foreach (var cachedViewModel in _cachedViewModels)
                foreach (var viewModel in cachedViewModel.Value)
                    list.Add(viewModel);
            return list;
        }

        /// <summary>
        ///     Removes the view model from cache.
        /// </summary>
        public virtual bool Invalidate(IViewModel viewModel, IDataContext context)
        {
            bool clear = false;
            foreach (var cachedViewModel in _cachedViewModels)
            {
                int indexOf = cachedViewModel.Value.IndexOf(viewModel);
                if (indexOf != -1)
                {
                    cachedViewModel.Value.RemoveAt(indexOf);
                    clear = true;
                }
            }
            return clear;
        }

        /// <summary>
        ///     Clears the cache.
        /// </summary>
        public virtual IList<IViewModel> Invalidate(IDataContext context)
        {
            var viewModels = GetViewModels(context);
            _cachedViewModels.Clear();
            return viewModels;
        }

        #endregion

        #region Methods

        private static object GetView(object view)
        {
            var viewWrapper = view as IViewWrapper;
            if (viewWrapper == null)
                return view;
            return viewWrapper.View;
        }

        #endregion
    }
}