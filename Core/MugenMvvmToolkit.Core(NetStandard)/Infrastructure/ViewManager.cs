#region Copyright

// ****************************************************************************
// <copyright file="ViewManager.cs">
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
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ViewManager : IViewManager
    {
        #region Fields

        protected const string ViewManagerCreatorPath = "@#vcreator";
        private readonly IThreadManager _threadManager;
        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly IWrapperManager _wrapperManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager([NotNull] IThreadManager threadManager,
            [NotNull] IViewMappingProvider viewMappingProvider, [NotNull] IWrapperManager wrapperManager)
        {
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(viewMappingProvider, nameof(viewMappingProvider));
            Should.NotBeNull(viewMappingProvider, nameof(wrapperManager));
            _threadManager = threadManager;
            _viewMappingProvider = viewMappingProvider;
            _wrapperManager = wrapperManager;
        }

        #endregion

        #region Properties

        protected IThreadManager ThreadManager => _threadManager;

        protected IViewMappingProvider ViewMappingProvider => _viewMappingProvider;

        protected IWrapperManager WrapperManager => _wrapperManager;

        #endregion

        #region Implementation of IViewManager

        public Task<object> GetViewAsync(IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var tcs = new TaskCompletionSource<object>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                if (context == null)
                    context = DataContext.Empty;
                IViewMappingItem mappingItem;
                object view = GetView(viewModel, context, out mappingItem);
                ViewCreated?.Invoke(this, new ViewCreatedEventArgs(view, viewModel, mappingItem, context));
                tcs.SetResult(view);
            });
            return tcs.Task;
        }

        public Task<object> GetViewAsync(IViewMappingItem viewMapping, IDataContext context = null)
        {
            Should.NotBeNull(viewMapping, nameof(viewMapping));
            var tcs = new TaskCompletionSource<object>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                if (context == null)
                    context = DataContext.Empty;
                object view = GetView(viewMapping, context);
                ViewCreated?.Invoke(this, new ViewCreatedEventArgs(view, null, viewMapping, context));
                tcs.SetResult(view);
            });
            return tcs.Task;
        }

        public Task InitializeViewAsync(IViewModel viewModel, object view, IDataContext context = null)
        {
            var tcs = new TaskCompletionSource<object>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                if (context == null)
                    context = DataContext.Empty;
                view = ToolkitExtensions.GetUnderlyingView<object>(view);
                var oldView = viewModel.GetCurrentView<object>(false);
                if (ReferenceEquals(oldView, view))
                {
                    tcs.SetResult(null);
                    return;
                }
                if (oldView != null)
                    CleanupViewInternal(viewModel, oldView, context);
                InitializeView(viewModel, view, context);
                ViewInitialized?.Invoke(this, new ViewInitializedEventArgs(view, viewModel, context));
                tcs.SetResult(null);
            });
            return tcs.Task;
        }

        public Task CleanupViewAsync(IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            object view = viewModel.GetCurrentView<object>(false);
            if (view == null)
                return Empty.Task;
            var tcs = new TaskCompletionSource<object>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                if (context == null)
                    context = DataContext.Empty;
                CleanupViewInternal(viewModel, ToolkitExtensions.GetUnderlyingView<object>(view), context);
                tcs.SetResult(null);
            }, OperationPriority.Low);
            return tcs.Task;
        }

        public event EventHandler<IViewManager, ViewCreatedEventArgs> ViewCreated;

        public event EventHandler<IViewManager, ViewInitializedEventArgs> ViewInitialized;

        public event EventHandler<IViewManager, ViewClearedEventArgs> ViewCleared;

        #endregion

        #region Methods

        protected virtual object GetView([NotNull] IViewModel viewModel, [NotNull] IDataContext context, out IViewMappingItem mappingItem)
        {
            string viewBindingName = viewModel.GetViewName(context);
            Type vmType = viewModel.GetType();
            mappingItem = ViewMappingProvider.FindMappingForViewModel(vmType, viewBindingName, true);
            return GetView(mappingItem, context);
        }

        protected virtual object GetView([NotNull] IViewMappingItem viewMapping, [NotNull] IDataContext context)
        {
            object viewObj = ServiceProvider.Get(viewMapping.ViewType);
            if (ApplicationSettings.ViewManagerDisposeView)
                ServiceProvider.AttachedValueProvider.SetValue(viewObj, ViewManagerCreatorPath, null);
            if (Tracer.TraceInformation)
                Tracer.Info("The view {0} for the view-model {1} was created.", viewObj.GetType(), viewMapping.ViewModelType);
            return viewObj;
        }

        protected virtual void InitializeView([NotNull] IViewModel viewModel, [CanBeNull] object view,
            [NotNull] IDataContext context)
        {
            InitializeViewInternal(viewModel, view);
            PropertyInfo viewProperty = ReflectionExtensions.GetViewProperty(viewModel.GetType());
            if (viewProperty == null)
                return;

            if (view != null && !viewProperty.PropertyType.IsInstanceOfType(view) &&
                WrapperManager.CanWrap(view.GetType(), viewProperty.PropertyType, context))
                view = WrapperManager.Wrap(view, viewProperty.PropertyType, context);
            viewProperty.SetValue(viewModel, view);
        }

        protected virtual void CleanupView([NotNull] IViewModel viewModel, [NotNull] object view,
            [NotNull] IDataContext context)
        {
            InitializeViewInternal(null, view);
            viewModel.Settings.Metadata.Remove(ViewModelConstants.View);
            ReflectionExtensions.GetViewProperty(viewModel.GetType())?.SetValue<object>(viewModel, null);

            viewModel.Unsubscribe(ToolkitExtensions.GetUnderlyingView<object>(view));
            if (ApplicationSettings.ViewManagerDisposeView && ServiceProvider.AttachedValueProvider.Contains(view, ViewManagerCreatorPath))
            {
                ServiceProvider.AttachedValueProvider.Clear(view, ViewManagerCreatorPath);
                (view as IDisposable)?.Dispose();
            }
        }

        private void CleanupViewInternal(IViewModel viewModel, object view, IDataContext context)
        {
            CleanupView(viewModel, view, context);
            ViewCleared?.Invoke(this, new ViewClearedEventArgs(view, viewModel, context));
        }

        private static void InitializeViewInternal(IViewModel viewModel, object view)
        {
            if (view == null)
                return;
            if (viewModel != null)
            {
                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.View, ServiceProvider.WeakReferenceFactory(view));
                viewModel.Subscribe(view);
            }

            if (viewModel != null || ApplicationSettings.ViewManagerClearDataContext)
                ToolkitExtensions.SetDataContext(view, viewModel);
            ReflectionExtensions.GetViewModelPropertySetter(view.GetType())?.Invoke(view, viewModel);
        }

        #endregion
    }
}
