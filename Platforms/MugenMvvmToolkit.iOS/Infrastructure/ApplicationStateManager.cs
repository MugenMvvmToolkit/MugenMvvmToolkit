#region Copyright

// ****************************************************************************
// <copyright file="ApplicationStateManager.cs">
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

        [Preserve(Conditional = true)]
        public ApplicationStateManager([NotNull] ISerializer serializer, [NotNull] IViewModelProvider viewModelProvider,
            [NotNull] IViewManager viewManager, [NotNull] IViewModelPresenter viewModelPresenter)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(viewModelPresenter, nameof(viewModelPresenter));
            _serializer = serializer;
            _viewModelProvider = viewModelProvider;
            _viewManager = viewManager;
            _viewModelPresenter = viewModelPresenter;
        }

        #endregion

        #region Properties

        protected ISerializer Serializer => _serializer;

        protected IViewModelProvider ViewModelProvider => _viewModelProvider;

        protected IViewManager ViewManager => _viewManager;

        protected IViewModelPresenter ViewModelPresenter => _viewModelPresenter;

        [CanBeNull]
        public static Func<Type, NSCoder, IDataContext, UIViewController> ViewControllerFactory { get; set; }

        #endregion

        #region Implementation of IApplicationStateManager

        public void EncodeState(NSObject item, NSCoder state, IDataContext context = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(state, nameof(state));
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

        public void DecodeState(NSObject item, NSCoder state, IDataContext context = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(state, nameof(state));
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
                    controller = (UIViewController)ServiceProvider.Get(type);
            }
            controller.RestorationIdentifier = restorationIdentifier;
            return controller;
        }

        [NotNull]
        protected virtual IDataContext RestoreViewModelState([NotNull] NSObject item, [NotNull] NSCoder coder, [NotNull] IDataContext context)
        {
            byte[] bytes = coder.DecodeBytes(VmStateKey);
            if (bytes == null)
                return DataContext.Empty;
            using (var ms = new MemoryStream(bytes))
                return (IDataContext)_serializer.Deserialize(ms);
        }

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
            var viewModel = _viewModelProvider.RestoreViewModel(viewModelState, context, true);
            _viewManager.InitializeViewAsync(viewModel, item, context).WithTaskExceptionHandler(this);
            _viewModelPresenter.Restore(viewModel, context);
        }

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
