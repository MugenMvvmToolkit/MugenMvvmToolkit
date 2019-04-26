using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public sealed class ViewAwareInitializerViewManagerListener : IViewManagerListener
    {
        #region Fields

        private readonly IReflectionManager _reflectionManager;
        private static readonly MethodInfo UpdateViewMethodInfo;
        private static readonly MethodInfo UpdateViewModelMethodInfo;

        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> TypeToInitializeDelegate;

        #endregion

        #region Constructors

        static ViewAwareInitializerViewManagerListener()
        {
            UpdateViewMethodInfo = typeof(MessengerHandlerSubscriber)
                .GetMethodsUnified(MemberFlags.StaticOnly)
                .FirstOrDefault(info => nameof(UpdateView).Equals(info.Name));
            UpdateViewModelMethodInfo = typeof(MessengerHandlerSubscriber)
                .GetMethodsUnified(MemberFlags.StaticOnly)
                .FirstOrDefault(info => nameof(UpdateViewModel).Equals(info.Name));
            Should.BeSupported(UpdateViewMethodInfo != null, nameof(UpdateViewMethodInfo));
            Should.BeSupported(UpdateViewModelMethodInfo != null, nameof(UpdateViewModelMethodInfo));
            TypeToInitializeDelegate = new Dictionary<Type, Func<object?, object?[], object?>>(MemberInfoEqualityComparer.Instance);
        }

        [Preserve(Conditional = true)]
        public ViewAwareInitializerViewManagerListener(IReflectionManager reflectionManager)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            _reflectionManager = reflectionManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
        }

        public void OnViewCreated(IViewManager viewManager, object view, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
        }

        public void OnViewInitialized(IViewManager viewManager, IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            GetUpdateViewMethod(viewModel, viewInfo.View)?.Invoke(this, new[] { viewModel, viewInfo, metadata, Default.FalseObject });//todo initialize wrappers
        }

        public void OnViewCleared(IViewManager viewManager, IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            GetUpdateViewMethod(viewModel, viewInfo.View)?.Invoke(this, new[] { viewModel, viewInfo, metadata, Default.TrueObject });//todo initialize wrappers
        }

        public int GetPriority(object source)
        {
            return Priority;
        }

        #endregion

        #region Methods

        private Func<object?, object?[], object?>? GetUpdateViewMethod(IViewModelBase viewModel, object view)
        {
            Func<object?, object?[], object?>? viewFunc;
            Func<object?, object?[], object?>? viewModelFunc;
            lock (TypeToInitializeDelegate)
            {
                var viewType = view.GetType();
                if (!TypeToInitializeDelegate.TryGetValue(viewType, out viewFunc))
                {
                    viewFunc = GetDelegate(viewType, typeof(IViewModelAwareView<>), nameof(IViewModelAwareView<IViewModelBase>.ViewModel), UpdateViewModelMethodInfo);
                    TypeToInitializeDelegate[viewType] = viewFunc;
                }

                var viewModelType = viewModel.GetType();
                if (!TypeToInitializeDelegate.TryGetValue(viewModelType, out viewModelFunc))
                {
                    viewModelFunc = GetDelegate(viewType, typeof(IViewAwareViewModel<>), nameof(IViewAwareViewModel<object>.View), UpdateViewMethodInfo);
                    TypeToInitializeDelegate[viewModelType] = viewModelFunc;
                }
            }

            if (viewFunc != null && viewModelFunc != null)
                return viewFunc + viewModelFunc;
            if (viewFunc != null)
                return viewFunc;
            return viewModelFunc;
        }

        [Preserve(Conditional = true)]
        internal void UpdateView<TView>(IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata, bool clear)
            where TView : class
        {
            if (viewModel is IViewAwareViewModel<TView> awareViewModel)
            {
                if (clear)
                {
                    awareViewModel.View = null;
                    return;
                }

                var wrappedView = viewInfo.TryWrap<TView>(metadata);
                if (wrappedView != null)
                    awareViewModel.View = wrappedView;
            }
        }

        [Preserve(Conditional = true)]
        internal void UpdateViewModel<TViewModel>(object viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata, bool clear)
            where TViewModel : class, IViewModelBase
        {
            if (viewInfo.View is IViewModelAwareView<TViewModel> awareView)
            {
                if (clear)
                    awareView.ViewModel = null;
                else if (viewModel is TViewModel vm)
                    awareView.ViewModel = vm;
            }
        }

        private Func<object?, object?[], object?>? GetDelegate(Type targetType, Type interfaceType, string propertyName, MethodInfo method)
        {
            Func<object?, object?[], object?>? result = null;
            foreach (var @interface in targetType.GetInterfacesUnified().Where(type => type.IsGenericTypeUnified()))
            {
                if (@interface.GetGenericTypeDefinition() != interfaceType)
                    continue;
                var propertyInfo = @interface.GetPropertyUnified(propertyName, MemberFlags.InstancePublic);
                if (propertyInfo == null)
                    continue;
                var methodDelegate = _reflectionManager.GetMethodDelegate(method.MakeGenericMethod(propertyInfo.PropertyType));
                if (result == null)
                    result = methodDelegate;
                else
                    result += methodDelegate;
            }

            return result;
        }

        #endregion
    }
}