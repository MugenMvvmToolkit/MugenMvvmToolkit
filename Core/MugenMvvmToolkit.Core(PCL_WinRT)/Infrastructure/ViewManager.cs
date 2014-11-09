#region Copyright
// ****************************************************************************
// <copyright file="ViewManager.cs">
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
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the default implemenation of <see cref="IViewManager" />
    /// </summary>
    public class ViewManager : IViewManager
    {
        #region Nested types

        private sealed class ViewWrapper : IViewWrapper, IDisposable
        {
            #region Fields

            private readonly object _view;
            private readonly Type _viewType;

            #endregion

            #region Constructors

            public ViewWrapper(object view)
            {
                Should.NotBeNull(view, "view");
                _view = view;
                _viewType = view.GetType();
            }

            #endregion

            #region Implementation of IViewWrapper

            public Type ViewType
            {
                get { return _viewType; }
            }

            public object View
            {
                get { return _view; }
            }

            public void Dispose()
            {
                var disposable = _view as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string AttachedViewMember = "~ATT~@$";
        private const string ViewManagerCreatorPath = "@#vcreator";
        private readonly IThreadManager _threadManager;
        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly Func<object, object, IView> _wrapperDelegate;
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
            [NotNull] IViewMappingProvider viewMappingProvider)
        {
            Should.NotBeNull(threadManager, "threadManager");
            Should.NotBeNull(viewMappingProvider, "viewMappingProvider");
            _threadManager = threadManager;
            _viewMappingProvider = viewMappingProvider;
            _wrapperDelegate = (o, o1) => WrapToViewInternal(o, (IDataContext)o1);
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
                _getDataContext = item =>
                {
                    var wrapper = item as IViewWrapper;
                    if (wrapper != null)
                        item = wrapper.View;
                    return value(item);
                };
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
                _setDataContext = (item, context) =>
                {
                    var wrapper = item as IViewWrapper;
                    if (wrapper != null)
                        item = wrapper.View;
                    value(item, context);
                };
            }
        }

        /// <summary>
        ///     Gets or sets the default value that indicates that view manager should always create new view.
        /// </summary>
        public static bool AlwaysCreateNewView { get; set; }

        /// <summary>
        ///     Gets or sets property, that is responsible for auto dispose view when the view model disposing.
        /// </summary>
        public bool DisposeView { get; set; }

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

        #endregion

        #region Implementation of IViewManager

        /// <summary>
        ///     Gets the type of view wrapper.
        /// </summary>
        public Type GetViewType(Type viewType, IDataContext dataContext)
        {
            Should.NotBeNull(viewType, "viewType");
            if (typeof(IView).IsAssignableFrom(viewType))
                return viewType;
            if (dataContext == null)
                dataContext = DataContext.Empty;
            return GetViewTypeInternal(viewType, dataContext);
        }

        /// <summary>
        ///     Wraps the specified view object to a <see cref="IView" />.
        /// </summary>
        public IView WrapToView(object view, IDataContext dataContext)
        {
            Should.NotBeNull(view, "view");
            if (dataContext == null)
                dataContext = DataContext.Empty;
            return view as IView ??
                         ServiceProvider.AttachedValueProvider.GetOrAdd(view, AttachedViewMember, _wrapperDelegate, dataContext);
        }

        /// <summary>
        ///     Gets an instance of <see cref="IView" /> for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model which is now initialized.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IView" />.
        /// </returns>
        public Task<IView> GetViewAsync(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var tcs = new TaskCompletionSource<IView>();
            ThreadManager.InvokeOnUiThreadAsync(() => tcs.SetResult(GetView(viewModel, dataContext ?? DataContext.Empty)));
            return tcs.Task;
        }

        /// <summary>
        ///     Configures the specified view for the specified view-model.
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        public Task InitializeViewAsync(IViewModel viewModel, object view)
        {
            var tcs = new TaskCompletionSource<bool>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                InitializeView(viewModel, WrapToView(view, DataContext.Empty));
                tcs.SetResult(true);
            });
            return tcs.Task;
        }

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        public Task CleanupViewAsync(IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, "viewModel");
            IView view = viewModel.Settings.Metadata.GetData(ViewModelConstants.View);
            if (view == null)
                return Empty.Task;
            var tcs = new TaskCompletionSource<bool>();
            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                CleanupView(viewModel, view);
                tcs.SetResult(true);
            }, OperationPriority.Low);
            return tcs.Task;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets an instance of <see cref="IView" /> for the specified view model.
        /// </summary>
        public static IView GetOrCreateView(IViewModel vm, bool? alwaysCreateNewView,
            [CanBeNull] IDataContext dataContext)
        {
            if (vm == null)
                return null;

            IView view;
            if (!alwaysCreateNewView.GetValueOrDefault(AlwaysCreateNewView))
            {
                view = vm.Settings.Metadata.GetData(ViewModelConstants.View);
                if (view != null)
                    return view;
            }

            //NOTE: SYNC INVOKE.
            var viewManager = vm.GetIocContainer(true).Get<IViewManager>();
            view = viewManager.GetViewAsync(vm, dataContext).Result;
            viewManager.InitializeViewAsync(vm, view);
            return view;
        }

        /// <summary>
        ///     Gets the type of view wrapper.
        /// </summary>
        protected virtual Type GetViewTypeInternal([NotNull] Type viewType, [NotNull] IDataContext dataContext)
        {
            return typeof(ViewWrapper);
        }

        /// <summary>
        ///     Wraps the specified view object to a <see cref="IView" />.
        /// </summary>
        protected virtual IView WrapToViewInternal([NotNull] object view, [NotNull] IDataContext dataContext)
        {
            return new ViewWrapper(view);
        }

        /// <summary>
        ///     Gets an instance of <see cref="IView" /> for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model which is now initialized.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IView" />.
        /// </returns>
        protected virtual IView GetView(IViewModel viewModel, IDataContext dataContext)
        {
            var viewBindingName = viewModel.GetViewName(dataContext);
            Type viewType = viewModel.GetType();
            IViewMappingItem mappingItem = ViewMappingProvider.FindMappingForViewModel(viewType, viewBindingName, true);
            object viewObj = viewModel.GetIocContainer(true).Get(mappingItem.ViewType);
            var view = viewObj as IView;
            if (view == null)
                view = WrapToView(viewObj, dataContext);
            else if (DisposeView)
                ServiceProvider.AttachedValueProvider.SetValue(viewObj, ViewManagerCreatorPath, null);
            Tracer.Info("The view {0} for the view-model {1} was created.", view.GetUnderlyingView().GetType(), viewModel.GetType());
            return view;
        }

        /// <summary>
        ///     Configures the specified view for the specified view-model.
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        protected virtual void InitializeView(IViewModel viewModel, IView view)
        {
            IView oldView = viewModel.Settings.Metadata.GetData(ViewModelConstants.View);
            if (ReferenceEquals(oldView, view))
                return;
            CleanupViewInternal(oldView);
            InitializeViewInternal(viewModel, view);
            PropertyInfo viewProperty = ReflectionExtensions.GetViewProperty(viewModel.GetType());
            if (viewProperty != null)
                viewProperty.SetValueEx(viewModel, view);
        }

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        protected virtual void CleanupView(IViewModel viewModel, IView view)
        {
            CleanupViewInternal(view);
            viewModel.Settings.Metadata.Remove(ViewModelConstants.View);
            PropertyInfo viewProperty = ReflectionExtensions.GetViewProperty(viewModel.GetType());
            if (viewProperty != null && viewProperty.GetValueEx<IView>(viewModel) != null)
                viewProperty.SetValueEx<IView>(viewModel, null);

            if (view != null)
            {
                viewModel.Unsubscribe(view.GetUnderlyingView());

                if (DisposeView)
                {
                    IDisposable disposable = null;
                    var wrapper = view as IViewWrapper;
                    if (wrapper == null)
                    {
                        if (ServiceProvider.AttachedValueProvider.Contains(view, ViewManagerCreatorPath))
                        {
                            ServiceProvider.AttachedValueProvider.Clear(view, ViewManagerCreatorPath);
                            disposable = view as IDisposable;
                        }
                    }
                    else
                        disposable = view as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
        }

        /// <summary>
        ///     Configures the specified view to the specified view-model.
        /// </summary>
        protected static void InitializeViewInternal(IViewModel viewModel, IView view)
        {
            if (view == null)
                return;
            var underlyingView = view.GetUnderlyingView();
            if (viewModel != null)
                viewModel.Subscribe(underlyingView);
            SetDataContext(underlyingView, viewModel);
            Action<object, IViewModel> propertySetter = ReflectionExtensions.GetViewModelPropertySetter(underlyingView.GetType());
            if (propertySetter != null)
                propertySetter(underlyingView, viewModel);

            if (viewModel != null)
                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.View, view);
        }

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="view">The specified view.</param>
        protected static void CleanupViewInternal(IView view)
        {
            InitializeViewInternal(null, view);
        }

        #endregion
    }
}