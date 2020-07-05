using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelViewAwareInitializer : IViewLifecycleDispatcherComponent, IHasPriority, IComponentCollectionChangedListener
    {
        #region Fields

        private readonly IReflectionManager? _reflectionManager;
        private readonly IWrapperManager? _wrapperManager;

        private static readonly TypeLightDictionary<object?> UpdateViewDelegates = new TypeLightDictionary<object?>(37);
        private static readonly TypeLightDictionary<object?> UpdateViewModelDelegates = new TypeLightDictionary<object?>(37);
        private static readonly MethodInfo UpdateViewMethodInfo = typeof(ViewModelViewAwareInitializer).GetMethodOrThrow(nameof(TryUpdateView), BindingFlagsEx.InstancePublic);
        private static readonly MethodInfo UpdateViewModelMethodInfo = typeof(ViewModelViewAwareInitializer).GetMethodOrThrow(nameof(TryUpdateViewModel), BindingFlagsEx.InstancePublic);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelViewAwareInitializer(IWrapperManager? wrapperManager = null, IReflectionManager? reflectionManager = null)
        {
            _wrapperManager = wrapperManager;
            _reflectionManager = reflectionManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            TryUpdateViewModel(component, ((IView)collection.Owner).ViewModel);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            TryUpdateViewModel(component, null);
        }

        public void OnLifecycleChanged<TState>(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (!(view is IView viewImp))
                return;
            if (lifecycleState == ViewLifecycleState.Initialized)
            {
                viewImp.Components.AddComponent(this);
                TryUpdateViewModel(viewImp.Target, viewImp.ViewModel);
                var components = viewImp.GetComponents<object>();
                for (var i = 0; i < components.Length; i++)
                    TryUpdateViewModel(components[i], viewImp.ViewModel);
                TryUpdateView(viewImp.ViewModel, viewImp, metadata);
            }
            else if (lifecycleState == ViewLifecycleState.Clearing)
            {
                TryUpdateView(viewImp.ViewModel, null, metadata);
                TryUpdateViewModel(viewImp.Target, null);
                var components = viewImp.GetComponents<object>();
                for (var i = 0; i < components.Length; i++)
                    TryUpdateViewModel(components[i], null);
                viewImp.Components.RemoveComponent(this);
            }
        }

        #endregion

        #region Methods

        [Preserve(Conditional = true)]
        public void TryUpdateView<TView>(IViewAwareViewModel<TView> viewModel, IView? view, IReadOnlyMetadataContext? metadata) where TView : class
        {
            if (view == null)
                viewModel.View = null;
            else if (view.Target is TView v)
                viewModel.View = v;
            else if (view.CanWrap<TView>(metadata, _wrapperManager))
                viewModel.View = view.Wrap<TView>(metadata, _wrapperManager);
        }

        [Preserve(Conditional = true)]
        public void TryUpdateViewModel<TViewModel>(IViewModelAwareView<TViewModel> view, IViewModelBase? viewModel) where TViewModel : class, IViewModelBase
        {
            if (viewModel == null)
                view.ViewModel = null;
            else if (viewModel is TViewModel vm)
                view.ViewModel = vm;
        }

        private void TryUpdateView(IViewModelBase viewModel, IView? view, IReadOnlyMetadataContext? metadata)
        {
            var vmType = viewModel.GetType();
            object? delegates;
            lock (UpdateViewDelegates)
            {
                if (!UpdateViewDelegates.TryGetValue(vmType, out delegates))
                {
                    delegates = GetDelegates<Action<ViewModelViewAwareInitializer, object, IView?, IReadOnlyMetadataContext?>>(vmType, typeof(IViewAwareViewModel<>), nameof(IViewAwareViewModel<object>.View), UpdateViewMethodInfo).GetRawValue();
                    UpdateViewDelegates[vmType] = delegates;
                }
            }

            var list = ItemOrList<Action<ViewModelViewAwareInitializer, object, IView?, IReadOnlyMetadataContext?>, List<Action<ViewModelViewAwareInitializer, object, IView?, IReadOnlyMetadataContext?>>>.FromRawValue(delegates);
            for (var i = 0; i < list.Count(); i++)
                list.Get(i).Invoke(this, viewModel, view, metadata);
        }

        private void TryUpdateViewModel(object view, IViewModelBase? viewModel)
        {
            var vType = view.GetType();
            object? delegates;
            lock (UpdateViewModelDelegates)
            {
                if (!UpdateViewModelDelegates.TryGetValue(vType, out delegates))
                {
                    delegates = GetDelegates<Action<ViewModelViewAwareInitializer, object, IViewModelBase?>>(vType, typeof(IViewModelAwareView<>), nameof(IViewModelAwareView<IViewModelBase>.ViewModel), UpdateViewModelMethodInfo).GetRawValue();
                    UpdateViewModelDelegates[vType] = delegates;
                }
            }

            var list = ItemOrList<Action<ViewModelViewAwareInitializer, object, IViewModelBase?>, List<Action<ViewModelViewAwareInitializer, object, IViewModelBase?>>>.FromRawValue(delegates);
            for (var i = 0; i < list.Count(); i++)
                list.Get(i).Invoke(this, view, viewModel);
        }

        private ItemOrList<TInvoker, List<TInvoker>> GetDelegates<TInvoker>(Type targetType, Type interfaceType, string propertyName, MethodInfo method) where TInvoker : Delegate
        {
            ItemOrList<TInvoker, List<TInvoker>> result = default;
            var interfaces = targetType.GetInterfaces();
            for (var index = 0; index < interfaces.Length; index++)
            {
                var @interface = interfaces[index];
                if (!@interface.IsGenericType || @interface.GetGenericTypeDefinition() != interfaceType)
                    continue;
                var propertyInfo = @interface.GetProperty(propertyName, BindingFlagsEx.InstancePublic);
                if (propertyInfo != null)
                {
                    var methodInvoker = method.MakeGenericMethod(propertyInfo.PropertyType).GetMethodInvoker<TInvoker>(_reflectionManager);
                    result.Add(methodInvoker);
                }
            }

            return result;
        }

        #endregion
    }
}