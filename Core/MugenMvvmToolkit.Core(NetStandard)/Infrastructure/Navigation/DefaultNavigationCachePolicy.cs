#region Copyright

// ****************************************************************************
// <copyright file="DefaultNavigationCachePolicy.cs">
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
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    public class DefaultNavigationCachePolicy : INavigationCachePolicy
    {
        #region Fields

        private readonly Dictionary<Type, List<IViewModel>> _cachedViewModels;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public DefaultNavigationCachePolicy()
        {
            _cachedViewModels = new Dictionary<Type, List<IViewModel>>();
        }

        #endregion

        #region Implementation of INavigationCachePolicy

        public virtual void TryCacheViewModel(INavigationContext context, object view, IViewModel viewModel)
        {
            if (context.NavigationMode == NavigationMode.Back)
                return;
            view = ToolkitExtensions.GetUnderlyingView<object>(view);
            Type type = view.GetType();

            List<IViewModel> list;
            if (!_cachedViewModels.TryGetValue(type, out list))
            {
                list = new List<IViewModel>();
                _cachedViewModels[type] = list;
            }
            list.Insert(0, viewModel);
            if (Tracer.TraceInformation)
                Tracer.Info("Navigation cache - the view model {0} was cached, navigation mode: {1}, view: {2}",
                    viewModel.GetType(), context.NavigationMode, type);
        }

        public virtual IViewModel TryTakeViewModelFromCache(INavigationContext context, object view)
        {
            view = ToolkitExtensions.GetUnderlyingView<object>(view);
            var type = view.GetType();
            List<IViewModel> list;
            if (!_cachedViewModels.TryGetValue(type, out list) || list == null || list.Count == 0)
            {
                if (Tracer.TraceInformation)
                    Tracer.Info("Navigation cache - the view model for the view {0} is not found in the cache, navigation mode: {1}", type, context.NavigationMode);
                return null;
            }
            IViewModel vm = list[0];
            list.RemoveAt(0);
            if (list.Count == 0)
                _cachedViewModels.Remove(type);
            if (Tracer.TraceInformation)
                Tracer.Info("Navigation cache - the view model {0} for the view {1} was taken from the cache, navigation mode: {2}",
                    vm.GetType(), type, context.NavigationMode);
            return vm;
        }

        public virtual IList<IViewModel> GetViewModels(IDataContext context)
        {
            var list = new List<IViewModel>();
            foreach (var cachedViewModel in _cachedViewModels)
                foreach (var viewModel in cachedViewModel.Value)
                    list.Add(viewModel);
            return list;
        }

        public virtual bool Invalidate(IViewModel viewModel, IDataContext context)
        {
            bool clear = false;
            List<Type> toRemove = null;
            foreach (var cachedViewModel in _cachedViewModels)
            {
                var viewModels = cachedViewModel.Value;
                int indexOf = viewModels.IndexOf(viewModel);
                if (indexOf != -1)
                {
                    viewModels.RemoveAt(indexOf);
                    clear = true;
                    if (viewModels.Count == 0)
                    {
                        if (toRemove == null)
                            toRemove = new List<Type>();
                        toRemove.Add(cachedViewModel.Key);
                    }
                }
            }
            if (toRemove != null)
            {
                foreach (var type in toRemove)
                    _cachedViewModels.Remove(type);
            }
            return clear;
        }

        public virtual IList<IViewModel> Invalidate(IDataContext context)
        {
            var viewModels = GetViewModels(context);
            _cachedViewModels.Clear();
            return viewModels;
        }

        #endregion
    }
}
