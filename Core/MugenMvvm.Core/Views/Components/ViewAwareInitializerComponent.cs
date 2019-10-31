using System;
using System.Linq;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewAwareInitializerComponent : IViewManagerListener, IHasPriority //todo review delegate type
    {
        #region Fields

        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;

        private static readonly MethodInfo UpdateViewMethodInfo = typeof(ViewAwareInitializerComponent).GetMethodOrThrow(nameof(UpdateView), BindingFlagsEx.StaticOnly);
        private static readonly MethodInfo UpdateViewModelMethodInfo = typeof(ViewAwareInitializerComponent).GetMethodOrThrow(nameof(UpdateViewModel), BindingFlagsEx.StaticOnly);

        private static readonly TypeLightDictionary<Func<object?, object?[], object?>?> TypeToInitializeDelegate =
            new TypeLightDictionary<Func<object?, object?[], object?>?>(17);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewAwareInitializerComponent(IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _reflectionDelegateProvider = reflectionDelegateProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IMetadataContext metadata)
        {
        }

        public void OnViewCreated(IViewManager viewManager, object view, IViewModelBase viewModel, IMetadataContext metadata)
        {
        }

        public void OnViewInitialized(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            GetUpdateViewMethod(viewModel, viewInfo.View)?.Invoke(this, new[] {viewModel, viewInfo, metadata, BoxingExtensions.FalseObject}); //todo initialize wrappers
        }

        public void OnViewCleared(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            GetUpdateViewMethod(viewModel, viewInfo.View)?.Invoke(this, new[] {viewModel, viewInfo, metadata, BoxingExtensions.TrueObject}); //todo initialize wrappers
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
                    viewFunc = GetDelegate(_reflectionDelegateProvider, viewType, typeof(IViewModelAwareView<>), nameof(IViewModelAwareView<IViewModelBase>.ViewModel),
                        UpdateViewModelMethodInfo);
                    TypeToInitializeDelegate[viewType] = viewFunc;
                }

                var viewModelType = viewModel.GetType();
                if (!TypeToInitializeDelegate.TryGetValue(viewModelType, out viewModelFunc))
                {
                    viewModelFunc = GetDelegate(_reflectionDelegateProvider, viewType, typeof(IViewAwareViewModel<>), nameof(IViewAwareViewModel<object>.View),
                        UpdateViewMethodInfo);
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
            if (!(viewModel is IViewAwareViewModel<TView> awareViewModel))
                return;

            if (clear)
            {
                awareViewModel.View = null;
                return;
            }

            var wrappedView = viewInfo.TryWrap<TView>(metadata);
            if (wrappedView != null)
                awareViewModel.View = wrappedView;
        }

        [Preserve(Conditional = true)]
        internal void UpdateViewModel<TViewModel>(object viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata, bool clear)
            where TViewModel : class, IViewModelBase
        {
            if (!(viewInfo.View is IViewModelAwareView<TViewModel> awareView))
                return;

            if (clear)
                awareView.ViewModel = null;
            else if (viewModel is TViewModel vm)
                awareView.ViewModel = vm;
        }

        private static Func<object?, object?[], object?>? GetDelegate(IReflectionDelegateProvider? reflectionDelegateProvider, Type targetType, Type interfaceType,
            string propertyName, MethodInfo method)
        {
            Func<object?, object?[], object?>? result = null;
            foreach (var @interface in targetType.GetInterfaces().Where(type => type.IsGenericType))
            {
                if (@interface.GetGenericTypeDefinition() != interfaceType)
                    continue;
                var propertyInfo = @interface.GetProperty(propertyName, BindingFlagsEx.InstancePublic);
                if (propertyInfo == null)
                    continue;
                var methodInvoker = method.MakeGenericMethod(propertyInfo.PropertyType).GetMethodInvoker(reflectionDelegateProvider);
                if (result == null)
                    result = methodInvoker;
                else
                    result += methodInvoker;
            }

            return result;
        }

        #endregion
    }
}