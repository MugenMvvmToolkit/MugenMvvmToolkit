#region Copyright

// ****************************************************************************
// <copyright file="ViewManager.cs">
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
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ViewManager : IViewManager
    {
        #region Fields

        protected const string ViewManagerCreatorPath = "@#vcreator";
        private static Func<object, object> _getDataContext;
        private static Action<object, object> _setDataContext;
        private readonly IThreadManager _threadManager;
        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly IWrapperManager _wrapperManager;

        #endregion

        #region Constructors

        static ViewManager()
        {
            AlwaysCreateNewView = false;
            GetDataContext = ReflectionExtensions.GetDataContext;
            SetDataContext = (o, o1) => ReflectionExtensions.SetDataContext(o, o1);
        }

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

        [NotNull]
        public static Func<object, object> GetDataContext
        {
            get { return _getDataContext; }
            set
            {
                Should.PropertyNotBeNull(value);
                _getDataContext = item => value(ToolkitExtensions.GetUnderlyingView<object>(item));
            }
        }

        [NotNull]
        public static Action<object, object> SetDataContext
        {
            get { return _setDataContext; }
            set
            {
                Should.PropertyNotBeNull(value);
                _setDataContext = (item, context) => value(ToolkitExtensions.GetUnderlyingView<object>(item), context);
            }
        }

        public static bool AlwaysCreateNewView { get; set; }

        public static bool DisposeView { get; set; }

        public static bool ClearDataContext { get; set; }

        public static Action<IViewManager, IViewModel, object, IDataContext> ViewCreated { get; set; }

        public static Action<IViewManager, IViewModel, object, IDataContext> ViewInitialized { get; set; }

        public static Action<IViewManager, IViewModel, object, IDataContext> ViewCleared { get; set; }

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
                object view = GetView(viewModel, context);
                ViewCreated?.Invoke(this, viewModel, view, context);
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
                ViewCreated?.Invoke(this, null, view, context);
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
                var oldView = viewModel.Settings.Metadata.GetData(ViewModelConstants.View);
                if (ReferenceEquals(oldView, view))
                {
                    tcs.SetResult(null);
                    return;
                }
                if (oldView != null)
                    CleanupViewInternal(viewModel, oldView, context);
                InitializeView(viewModel, view, context);
                ViewInitialized?.Invoke(this, viewModel, view, context);
                tcs.SetResult(null);
            });
            return tcs.Task;
        }

        public Task CleanupViewAsync(IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            object view = viewModel.Settings.Metadata.GetData(ViewModelConstants.View);
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

        #endregion

        #region Methods

        public static object GetOrCreateView([CanBeNull] IViewModel vm, bool? alwaysCreateNewView = null,
            IDataContext context = null)
        {
            if (vm == null)
                return null;

            object view;
            if (!alwaysCreateNewView.GetValueOrDefault(AlwaysCreateNewView))
            {
                view = vm.Settings.Metadata.GetData(ViewModelConstants.View);
                if (view != null)
                    return view;
            }

            //NOTE: SYNC INVOKE.
            var viewManager = vm.GetIocContainer(true).Get<IViewManager>();
            view = viewManager.GetViewAsync(vm, context).Result;
            viewManager.InitializeViewAsync(vm, view, context);
            return view;
        }

        protected virtual object GetView([NotNull] IViewModel viewModel, [NotNull] IDataContext context)
        {
            string viewBindingName = viewModel.GetViewName(context);
            Type vmType = viewModel.GetType();
            IViewMappingItem mappingItem = ViewMappingProvider.FindMappingForViewModel(vmType, viewBindingName, true);
            return GetView(mappingItem, context);
        }

        protected virtual object GetView([NotNull] IViewMappingItem viewMapping, [NotNull] IDataContext context)
        {
            object viewObj = ServiceProvider.Get(viewMapping.ViewType);
            if (DisposeView)
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
            PropertyInfo viewProperty = ReflectionExtensions.GetViewProperty(viewModel.GetType());
            if (viewProperty != null)
                viewProperty.SetValue<object>(viewModel, null);

            viewModel.Unsubscribe(ToolkitExtensions.GetUnderlyingView<object>(view));
            if (DisposeView && ServiceProvider.AttachedValueProvider.Contains(view, ViewManagerCreatorPath))
            {
                ServiceProvider.AttachedValueProvider.Clear(view, ViewManagerCreatorPath);
                var disposable = view as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        private void CleanupViewInternal(IViewModel viewModel, object view, IDataContext context)
        {
            CleanupView(viewModel, view, context);
            ViewCleared?.Invoke(this, viewModel, view, context);
        }

        private static void InitializeViewInternal(IViewModel viewModel, object view)
        {
            if (view == null)
                return;
            if (viewModel != null)
            {
                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.View, view);
                viewModel.Subscribe(view);
            }

            if (viewModel != null || ClearDataContext)
                SetDataContext(view, viewModel);
            Action<object, IViewModel> propertySetter = ReflectionExtensions.GetViewModelPropertySetter(view.GetType());
            if (propertySetter != null)
                propertySetter(view, viewModel);
        }

        #endregion
    }
}
