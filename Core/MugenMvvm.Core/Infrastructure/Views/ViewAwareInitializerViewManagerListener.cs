//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using MugenMvvm.Attributes;
//using MugenMvvm.Enums;
//using MugenMvvm.Infrastructure.Internal;
//using MugenMvvm.Interfaces.Metadata;
//using MugenMvvm.Interfaces.ViewModels;
//using MugenMvvm.Interfaces.Views;
//using MugenMvvm.Interfaces.Views.Infrastructure;
//using MugenMvvm.Interfaces.Wrapping;
//
//namespace MugenMvvm.Infrastructure.Views
//{
//    public class ViewAwareInitializerViewManagerListener : IViewManagerListener//todo rewrite to method invoke
//    {
//        #region Fields
//
//        private static readonly Dictionary<Type, PropertyInfo> ViewToViewModelInterface;
//        private static readonly Dictionary<Type, PropertyInfo> ViewModelToViewInterface;
//
//        #endregion
//
//        #region Constructors
//
//        static ViewAwareInitializerViewManagerListener()
//        {
//            ViewToViewModelInterface = new Dictionary<Type, PropertyInfo>(MemberInfoComparer.Instance);
//            ViewModelToViewInterface = new Dictionary<Type, PropertyInfo>(MemberInfoComparer.Instance);
//        }
//
//        [Preserve(Conditional = true)]
//        public ViewAwareInitializerViewManagerListener(IWrapperManager wrapperManager)
//        {
//            WrapperManager = wrapperManager;
//        }
//
//        #endregion
//
//        #region Properties
//
//        protected IWrapperManager WrapperManager { get; }
//
//        #endregion
//
//        #region Implementation of interfaces
//
//        public void OnViewInitialized(IViewManager viewManager, IViewModel viewModel, object view, IReadOnlyMetadataContext metadata)
//        {
//            var underlyingView = MugenExtensions.GetUnderlyingView<object>(view);
//            GetViewModelPropertySetter(underlyingView.GetType())?.SetValue(underlyingView, viewModel);
//
//            var viewProperty = GetViewProperty(viewModel.GetType());
//            if (viewProperty == null)
//                return;
//
//            if (viewProperty.PropertyType.IsInstanceOfTypeUnified(view))
//            {
//                viewProperty.SetValue(viewModel, view);
//                return;
//            }
//
//            if (!viewProperty.PropertyType.IsInstanceOfTypeUnified(underlyingView) && WrapperManager.CanWrap(underlyingView.GetType(), viewProperty.PropertyType, metadata))
//                view = WrapperManager.Wrap(underlyingView, viewProperty.PropertyType, metadata);
//            viewProperty.SetValue(viewModel, view);
//        }
//
//        public void OnViewCleared(IViewManager viewManager, IViewModel viewModel, object view, IReadOnlyMetadataContext metadata)
//        {
//            view = MugenExtensions.GetUnderlyingView<object>(view);
//            GetViewModelPropertySetter(view.GetType())?.SetValue(view, null);
//            GetViewProperty(viewModel.GetType())?.SetValue(viewModel, null);
//        }
//
//        #endregion
//
//        #region Methods
//
//        private static PropertyInfo GetViewModelPropertySetter(Type viewType)
//        {
//            lock (ViewToViewModelInterface)
//            {
//                if (!ViewToViewModelInterface.TryGetValue(viewType, out var result))
//                {
//                    foreach (var @interface in viewType.GetInterfacesUnified().Where(type => type.IsGenericTypeUnified()))
//                    {
//                        if (@interface.GetGenericTypeDefinition() != typeof(IViewModelAwareView<>)) continue;
//                        if (result != null)
//                            throw ExceptionManager.DuplicateInterface("view", "IViewModelAwareView<>", viewType);
//                        result = @interface.GetPropertyUnified(nameof(IViewModelAwareView<IViewModel>.ViewModel), MemberFlags.InstancePublic);
//                    }
//
//                    ViewToViewModelInterface[viewType] = result;
//                }
//
//                return result;
//            }
//        }
//
//        private static PropertyInfo GetViewProperty(Type viewModelType)
//        {
//            lock (ViewModelToViewInterface)
//            {
//                if (!ViewModelToViewInterface.TryGetValue(viewModelType, out var result))
//                {
//                    foreach (var @interface in viewModelType.GetInterfacesUnified().Where(type => type.IsGenericTypeUnified()))
//                    {
//                        if (@interface.GetGenericTypeDefinition() != typeof(IViewAwareViewModel<>)) continue;
//                        if (result != null)
//                            throw ExceptionManager.DuplicateInterface("view model", "IViewAwareViewModel<>", viewModelType);
//                        result = @interface.GetPropertyUnified(nameof(IViewAwareViewModel<object>.View), MemberFlags.InstancePublic);
//                    }
//
//                    ViewModelToViewInterface[viewModelType] = result;
//                }
//
//                return result;
//            }
//        }
//
//        #endregion
//    }
//}