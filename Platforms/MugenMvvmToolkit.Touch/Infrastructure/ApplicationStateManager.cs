#region Copyright
// ****************************************************************************
// <copyright file="ApplicationStateManager.cs">
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
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Infrastructure
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

        [CanBeNull]
        public static Func<Type, NSCoder, IDataContext, UIViewController> ViewControllerFactory { get; set; }

        #endregion

        #region Implementation of IApplicationStateManager

        /// <summary>
        ///     Occurs on save element state.
        /// </summary>
        public virtual void EncodeState(NSObject item, NSCoder state, IDataContext context = null)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(state, "state");
            var controller = item as UIViewController;
            if (controller != null && string.IsNullOrEmpty(controller.RestorationIdentifier))
                return;
            var view = item as UIView;
            if (view != null && string.IsNullOrEmpty(view.RestorationIdentifier))
                return;
            SaveState(item, Infrastructure.ViewManager.GetDataContext(item) as IViewModel, state);
        }

        /// <summary>
        ///     Occurs on load element state.
        /// </summary>
        public virtual void DecodeState(NSObject item, NSCoder state, IDataContext context = null)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(state, "state");
            RestoreState(item, state);
        }

        /// <summary>
        ///     Tries to restore view controller.
        /// </summary>
        public virtual UIViewController GetViewController(string[] restorationIdentifierComponents, NSCoder coder,
            IDataContext context = null)
        {
            string id = restorationIdentifierComponents.LastOrDefault();
            Type type = PlatformExtensions.GetTypeFromRestorationIdentifier(id);
            if (type == null)
                return null;
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
            controller.RestorationIdentifier = id;
            return controller;
        }

        #endregion

        #region Methods

        private void RestoreState(NSObject item, NSCoder coder)
        {
            RestoreNavigationParameter(item, coder);
            if (!coder.ContainsKey(VmStateKey) || !coder.ContainsKey(VmTypeKey))
                return;

            var vmTypeName = (NSString)coder.DecodeObject(VmTypeKey);
            if (vmTypeName == null)
                return;
            object dataContext = Infrastructure.ViewManager.GetDataContext(item);

            var vmType = Type.GetType(vmTypeName, false);
            if (vmType == null || (dataContext != null && dataContext.GetType().Equals(vmType)))
                return;

            byte[] bytes = coder.DecodeBytes(VmStateKey);
            IDataContext state;
            using (var ms = new MemoryStream(bytes))
                state = (IDataContext)_serializer.Deserialize(ms);
            var context = new DataContext
            {
                {InitializationConstants.ViewModelType, vmType}                
            };
            if (item is IModalView)
            {
                context.Add(DynamicViewModelWindowPresenter.RestoredViewConstant, item);
                context.Add(DynamicViewModelWindowPresenter.IsOpenViewConstant, true);
            }
            var viewModel = _viewModelProvider.RestoreViewModel(state, context, false);
            _viewManager.InitializeViewAsync(viewModel, item);
            _viewModelPresenter.Restore(viewModel, context);
        }

        private void SaveState(NSObject item, IViewModel viewModel, NSCoder coder)
        {
            object navigationParameter = (item as UIViewController).GetNavigationParameter();
            if (navigationParameter != null)
            {
                using (Stream stream = _serializer.Serialize(navigationParameter))
                    coder.Encode(stream.ToArray(), ParameterStateKey);
            }

            if (viewModel == null)
                return;
            coder.Encode(new NSString(viewModel.GetType().AssemblyQualifiedName), VmTypeKey);
            var state = _viewModelProvider.PreserveViewModel(viewModel, DataContext.Empty);
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