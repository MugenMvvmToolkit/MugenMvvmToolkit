using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewAwareInitializerComponent : IViewManagerListener, IHasPriority
    {
        #region Fields

        private static readonly MethodInfo UpdateViewMethodInfo = GetUpdateViewMethod();
        private static readonly MethodInfo UpdateViewModelMethodInfo = GetUpdateViewModelMethod();

        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> TypeToInitializeDelegate =
            new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewAwareInitializerComponent()
        {
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
            GetUpdateViewMethod(viewModel, viewInfo.View)?.Invoke(this, new[] { viewModel, viewInfo, metadata, Default.FalseObject }); //todo initialize wrappers
        }

        public void OnViewCleared(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            GetUpdateViewMethod(viewModel, viewInfo.View)?.Invoke(this, new[] { viewModel, viewInfo, metadata, Default.TrueObject }); //todo initialize wrappers
        }

        #endregion

        #region Methods

        private static MethodInfo GetUpdateViewMethod()
        {
            var m = typeof(MessengerHandlerSubscriber)
                .GetMethodsUnified(MemberFlags.StaticOnly)
                .FirstOrDefault(info => nameof(UpdateView).Equals(info.Name));
            Should.BeSupported(m != null, nameof(UpdateViewMethodInfo));
            return m!;
        }

        private static MethodInfo GetUpdateViewModelMethod()
        {
            var m = typeof(MessengerHandlerSubscriber)
                .GetMethodsUnified(MemberFlags.StaticOnly)
                .FirstOrDefault(info => nameof(UpdateViewModel).Equals(info.Name));
            Should.BeSupported(m != null, nameof(UpdateViewModelMethodInfo));
            return m!;
        }

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

        private static Func<object?, object?[], object?>? GetDelegate(Type targetType, Type interfaceType, string propertyName, MethodInfo method)
        {
            Func<object?, object?[], object?>? result = null;
            foreach (var @interface in targetType.GetInterfacesUnified().Where(type => type.IsGenericTypeUnified()))
            {
                if (@interface.GetGenericTypeDefinition() != interfaceType)
                    continue;
                var propertyInfo = @interface.GetPropertyUnified(propertyName, MemberFlags.InstancePublic);
                if (propertyInfo == null)
                    continue;
                var methodInvoker = method.MakeGenericMethod(propertyInfo.PropertyType).GetMethodInvoker();
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