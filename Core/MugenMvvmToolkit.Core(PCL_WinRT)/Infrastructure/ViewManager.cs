#region Copyright

// ****************************************************************************
// <copyright file="ViewManager.cs">
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
    /// <summary>
    ///     Represents the provider that allows to create a view for a view model
    /// </summary>
    public class ViewManager : IViewManager
    {
        #region Fields

        private const string ViewManagerCreatorPath = "@#vcreator";
        private readonly IThreadManager _threadManager;
        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly IWrapperManager _wrapperManager;
        private static Func<object, object> _getDataContext;
        private static Action<object, object> _setDataContext;

        #endregion

        #region Constructors

        static ViewManager()
        {
            AlwaysCreateNewView = false;
            GetDataContext = ReflectionExtensions.GetDataContext;
            SetDataContext = (o, o1) => ReflectionExtensions.SetDataContext(o, o1);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewManager" /> class.
        /// </summary>
        public ViewManager([NotNull] IThreadManager threadManager,
            [NotNull] IViewMappingProvider viewMappingProvider, [NotNull] IWrapperManager wrapperManager)
        {
            Should.NotBeNull(threadManager, "threadManager");
            Should.NotBeNull(viewMappingProvider, "viewMappingProvider");
            Should.NotBeNull(viewMappingProvider, "wrapperManager");
            _threadManager = threadManager;
            _viewMappingProvider = viewMappingProvider;
            _wrapperManager = wrapperManager;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the delegate that allows to get data context of view.
        /// </summary>
        [NotNull]
        public static Func<object, object> GetDataContext
        {
            get { return _getDataContext; }
            set
            {
                Should.PropertyBeNotNull(value);
                _getDataContext = item => value(ToolkitExtensions.GetUnderlyingView<object>(item));
            }
        }

        /// <summary>
        /// Gets or sets the delegate that allows to set data context of view.
        /// </summary>
        [NotNull]
        public static Action<object, object> SetDataContext
        {
            get { return _setDataContext; }
            set
            {
                Should.PropertyBeNotNull(value);
                _setDataContext = (item, context) => value(ToolkitExtensions.GetUnderlyingView<object>(item), context);
            }
        }

        /// <summary>
        ///     Gets or sets the default value that indicates that view manager should always create new view.
        /// </summary>
        public static bool AlwaysCreateNewView { get; set; }

        /// <summary>
        ///     Gets or sets property, that is responsible for auto dispose view when the view model disposing.
        /// </summary>
        public static bool DisposeView { get; set; }

        /// <summary>
        ///     Gets or sets property, that is responsible for clear view data context when the view model disposing.
        /// </summary>
        public static bool ClearDataContext { get; set; }

        /// <summary>
        ///     Occurs when view is created.
        /// </summary>
        public static Action<IViewManager, IViewModel, object, IDataContext> ViewCreated { get; set; }

        /// <summary>
        ///     Occurs when view is initialized.
        /// </summary>
        public static Action<IViewManager, IViewModel, object, IDataContext> ViewInitialized { get; set; }

        /// <summary>
        ///     Occurs when view is cleared.
        /// </summary>
        public static Action<IViewManager, IViewModel, object, IDataContext> ViewCleared { get; set; }

        /// <summary>
        ///     Gets the thread manager.
        /// </summary>
        protected IThreadManager ThreadManager
        {
            get { return _threadManager; }
        }

        /// <summary>
        ///     Gets the view mapping provider.
        /// </summary>
        protected IViewMappingProvider ViewMappingProvider
        {
            get { return _viewMappingProvider; }
        }

        /// <summary>
        ///     Gets the wrapper manager.
        /// </summary>
        protected IWrapperManager WrapperManager
        {
            get { return _wrapperManager; }
        }

        #endregion

        #region Implementation of IViewManager

        /// <summary>
        ///     Gets an instance of view object for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model which is now initialized.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        /// <returns>
        ///     An instance of view object.
        /// </returns>
        public Task<object> GetViewAsync(IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var tcs = new TaskCompletionSource<object>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                if (context == null)
                    context = DataContext.Empty;
                var view = GetView(viewModel, context);
                var handler = ViewCreated;
                if (handler != null)
                    handler(this, viewModel, view, context);
                tcs.SetResult(view);
            });
            return tcs.Task;
        }

        /// <summary>
        ///     Configures the specified view for the specified view-model.
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        public Task InitializeViewAsync(IViewModel viewModel, object view, IDataContext context = null)
        {
            var tcs = new TaskCompletionSource<object>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                if (context == null)
                    context = DataContext.Empty;
                view = ToolkitExtensions.GetUnderlyingView<object>(view);
                if (ReferenceEquals(viewModel.Settings.Metadata.GetData(ViewModelConstants.View), view))
                {
                    tcs.SetResult(null);
                    return;
                }
                InitializeView(viewModel, view, context);
                var handler = ViewInitialized;
                if (handler != null)
                    handler(this, viewModel, view, context);
                tcs.SetResult(null);
            });
            return tcs.Task;
        }

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        public Task CleanupViewAsync(IViewModel viewModel, IDataContext context = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var view = viewModel.Settings.Metadata.GetData(ViewModelConstants.View);
            if (view == null)
                return Empty.Task;
            var tcs = new TaskCompletionSource<object>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                if (context == null)
                    context = DataContext.Empty;
                view = ToolkitExtensions.GetUnderlyingView<object>(view);
                CleanupView(viewModel, view, context);
                var handler = ViewCleared;
                if (handler != null)
                    handler(this, viewModel, view, context);
                tcs.SetResult(null);
            }, OperationPriority.Low);
            return tcs.Task;
        }

        #endregion

        #region Methods

        /// <summary>
        ///    Gets an instance of view object for the specified view model.
        /// </summary>
        public static object GetOrCreateView([CanBeNull] IViewModel vm, bool? alwaysCreateNewView = null, IDataContext context = null)
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

        /// <summary>
        ///     Gets an instance of view object for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model which is now initialized.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        /// <returns>
        ///     An instance of view object.
        /// </returns>
        protected virtual object GetView([NotNull]IViewModel viewModel, [NotNull] IDataContext context)
        {
            var viewBindingName = viewModel.GetViewName(context);
            var vmType = viewModel.GetType();
            IViewMappingItem mappingItem = ViewMappingProvider.FindMappingForViewModel(vmType, viewBindingName, true);
            object viewObj = viewModel.GetIocContainer(true).Get(mappingItem.ViewType);
            if (DisposeView)
                ServiceProvider.AttachedValueProvider.SetValue(viewObj, ViewManagerCreatorPath, null);
            Tracer.Info("The view {0} for the view-model {1} was created.", viewObj.GetType(), vmType);
            return viewObj;
        }

        /// <summary>
        ///     Configures the specified view for the specified view-model.
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        protected virtual void InitializeView([NotNull]IViewModel viewModel, [CanBeNull] object view, [NotNull] IDataContext context)
        {
            var oldView = viewModel.Settings.Metadata.GetData(ViewModelConstants.View);
            InitializeViewInternal(null, oldView);
            InitializeViewInternal(viewModel, view);
            PropertyInfo viewProperty = ReflectionExtensions.GetViewProperty(viewModel.GetType());
            if (viewProperty == null)
                return;

            if (view != null && !viewProperty.PropertyType.IsInstanceOfType(view) &&
                _wrapperManager.CanWrap(view.GetType(), viewProperty.PropertyType, DataContext.Empty))
                view = _wrapperManager.Wrap(view, viewProperty.PropertyType, DataContext.Empty);
            viewProperty.SetValueEx(viewModel, view);
        }

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        protected virtual void CleanupView([NotNull] IViewModel viewModel, [NotNull] object view, [NotNull] IDataContext context)
        {
            InitializeViewInternal(null, view);
            viewModel.Settings.Metadata.Remove(ViewModelConstants.View);
            PropertyInfo viewProperty = ReflectionExtensions.GetViewProperty(viewModel.GetType());
            if (viewProperty != null)
                viewProperty.SetValueEx<object>(viewModel, null);

            viewModel.Unsubscribe(ToolkitExtensions.GetUnderlyingView<object>(view));
            if (DisposeView && ServiceProvider.AttachedValueProvider.Contains(view, ViewManagerCreatorPath))
            {
                ServiceProvider.AttachedValueProvider.Clear(view, ViewManagerCreatorPath);
                var disposable = view as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        /// <summary>
        ///     Configures the specified view to the specified view-model.
        /// </summary>
        protected static void InitializeViewInternal(IViewModel viewModel, object view)
        {
            if (view == null)
                return;
            if (viewModel != null)
                viewModel.Subscribe(view);

            if (viewModel != null || ClearDataContext)
                SetDataContext(view, viewModel);
            Action<object, IViewModel> propertySetter = ReflectionExtensions.GetViewModelPropertySetter(view.GetType());
            if (propertySetter != null)
                propertySetter(view, viewModel);

            if (viewModel != null)
                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.View, view);
        }

        #endregion
    }
}