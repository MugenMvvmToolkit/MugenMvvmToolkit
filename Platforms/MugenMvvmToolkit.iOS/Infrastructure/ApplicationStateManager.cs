#region Copyright

// ****************************************************************************
// <copyright file="ApplicationStateManager.cs">
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
using System.IO;
using System.Linq;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Infrastructure.Presenters;
using MugenMvvmToolkit.iOS.Interfaces;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.iOS.Views;
using MugenMvvmToolkit.Models;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure
{
    /// <summary>
    ///     Represents the base implementation of <see cref="IApplicationStateManager"/>.
    /// </summary>
    public class ApplicationStateManager : IApplicationStateManager
    {
        #region Fields

        private const string VmStateKey = "@`vmstate";
        private const string VmTypeKey = "~vmType`";
        private const string ParameterStateKey = "~param~";

        private readonly ISerializer _serializer;
        private readonly IViewManager _viewManager;
        private readonly IViewModelPresenter _viewModelPresenter;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationStateManager" /> class.
        /// </summary>
        public ApplicationStateManager([NotNull] ISerializer serializer, [NotNull] IViewModelProvider viewModelProvider,
            [NotNull] IViewManager viewManager, [NotNull] IViewModelPresenter viewModelPresenter)
        {
            Should.NotBeNull(serializer, "serializer");
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            Should.NotBeNull(viewManager, "viewManager");
            Should.NotBeNull(viewModelPresenter, "viewModelPresenter");
            _serializer = serializer;
            _viewModelProvider = viewModelProvider;
            _viewManager = viewManager;
            _viewModelPresenter = viewModelPresenter;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="ISerializer" />.
        /// </summary>
        protected ISerializer Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelProvider" />.
        /// </summary>
        protected IViewModelProvider ViewModelProvider
        {
            get { return _viewModelProvider; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewManager" />.
        /// </summary>
        protected IViewManager ViewManager
        {
            get { return _viewManager; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelPresenter" />.
        /// </summary>
        protected IViewModelPresenter ViewModelPresenter
        {
            get { return _viewModelPresenter; }
        }

        /// <summary>
        ///     Gets or sets the delegate that allows to create an instance of <see cref="UIViewController" />.
        /// </summary>
        [CanBeNull]
        public static Func<Type, NSCoder, IDataContext, UIViewController> ViewControllerFactory { get; set; }

        #endregion

        #region Implementation of IApplicationStateManager

        /// <summary>
        ///     Occurs on save element state.
        /// </summary>
        public void EncodeState(NSObject item, NSCoder state, IDataContext context = null)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(state, "state");
            var controller = item as UIViewController;
            if (controller != null && string.IsNullOrEmpty(controller.RestorationIdentifier))
                return;
            var view = item as UIView;
            if (view != null && string.IsNullOrEmpty(view.RestorationIdentifier))
                return;
            object navigationParameter = (item as UIViewController).GetNavigationParameter();
            if (navigationParameter != null)
            {
                using (Stream stream = _serializer.Serialize(navigationParameter))
                    state.Encode(stream.ToArray(), ParameterStateKey);
            }
            var viewModel = item.DataContext() as IViewModel;
            if (viewModel != null)
            {
                state.Encode(new NSString(viewModel.GetType().AssemblyQualifiedName), VmTypeKey);
                PreserveViewModel(viewModel, item, state, context ?? DataContext.Empty);
            }
        }

        /// <summary>
        ///     Occurs on load element state.
        /// </summary>
        public void DecodeState(NSObject item, NSCoder state, IDataContext context = null)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(state, "state");
            RestoreNavigationParameter(item, state);
            if (!state.ContainsKey(VmTypeKey))
                return;
            var vmTypeName = (NSString)state.DecodeObject(VmTypeKey);
            if (vmTypeName == null)
                return;

            object dataContext = item.DataContext();
            var vmType = Type.GetType(vmTypeName, false);
            if (vmType != null && (dataContext == null || !dataContext.GetType().Equals(vmType)))
            {
                if (context == null)
                    context = DataContext.Empty;
                RestoreViewModel(vmType, RestoreViewModelState(item, state, context), item, state, context);
            }
        }

        /// <summary>
        ///     Tries to restore view controller.
        /// </summary>
        public UIViewController GetViewController(string[] restorationIdentifierComponents, NSCoder coder, IDataContext context = null)
        {
            string id = restorationIdentifierComponents.LastOrDefault();
            Type type = PlatformExtensions.GetTypeFromRestorationIdentifier(id);
            if (type == null)
                return null;
            return GetViewControllerInternal(id, type, coder, context ?? DataContext.Empty);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Tries to restore view controller.
        /// </summary>
        [CanBeNull]
        protected virtual UIViewController GetViewControllerInternal([NotNull] string restorationIdentifier, [NotNull] Type type,
            [NotNull] NSCoder coder, [NotNull] IDataContext context)
        {
            UIViewController controller = null;
            Func<Type, NSCoder, IDataContext, UIViewController> factory = ViewControllerFactory;
            if (factory != null)
                controller = factory(type, coder, context);
            if (controller == null)
            {
                if (type == typeof(MvvmNavigationController))
                    controller = new MvvmNavigationController();
                else
                    controller = (UIViewController)ServiceProvider.IocContainer.Get(type);
            }
            controller.RestorationIdentifier = restorationIdentifier;
            return controller;
        }

        /// <summary>
        ///     Restores the view model state.
        /// </summary>
        [NotNull]
        protected virtual IDataContext RestoreViewModelState([NotNull] NSObject item, [NotNull] NSCoder coder, [NotNull] IDataContext context)
        {
            byte[] bytes = coder.DecodeBytes(VmStateKey);
            if (bytes == null)
                return DataContext.Empty;
            using (var ms = new MemoryStream(bytes))
                return (IDataContext)_serializer.Deserialize(ms);
        }

        /// <summary>
        ///     Restores the view model.
        /// </summary>
        protected virtual void RestoreViewModel([NotNull] Type viewModelType, [NotNull] IDataContext viewModelState, [NotNull] NSObject item, [NotNull] NSCoder coder,
            [NotNull] IDataContext context)
        {
            context = context.ToNonReadOnly();
            context.AddOrUpdate(InitializationConstants.ViewModelType, viewModelType);

            if (item is IModalView)
            {
                context.Add(DynamicViewModelWindowPresenter.RestoredViewConstant, item);
                context.Add(DynamicViewModelWindowPresenter.IsOpenViewConstant, true);
            }
            var viewModel = _viewModelProvider.RestoreViewModel(viewModelState, context, false);
            _viewManager.InitializeViewAsync(viewModel, item, context).WithTaskExceptionHandler(this);
            _viewModelPresenter.Restore(viewModel, context);
        }

        /// <summary>
        ///     Preserves the view model.
        /// </summary>
        protected virtual void PreserveViewModel([NotNull] IViewModel viewModel, NSObject item, [NotNull] NSCoder coder, [NotNull] IDataContext context)
        {
            var state = _viewModelProvider.PreserveViewModel(viewModel, context);
            using (var stream = _serializer.Serialize(state))
                coder.Encode(stream.ToArray(), VmStateKey);
        }

        private void RestoreNavigationParameter(NSObject item, NSCoder coder)
        {
            if (!coder.ContainsKey(ParameterStateKey))
                return;
            var controller = item as UIViewController;
            if (controller == null)
                return;

            byte[] bytes = coder.DecodeBytes(ParameterStateKey);
            using (var ms = new MemoryStream(bytes))
            {
                object parameter = _serializer.Deserialize(ms);
                controller.SetNavigationParameter(parameter);
            }
        }

        #endregion
    }
}