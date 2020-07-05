using System;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class TypeViewModelProvider : IViewModelProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IServiceProvider? _serviceProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public TypeViewModelProvider(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.Provider;

        #endregion

        #region Implementation of interfaces

        public IViewModelBase? TryGetViewModel<TRequest>(IViewModelManager viewModelManager, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TRequest>() || !(request is Type type))
                return null;

            var viewModel = (IViewModelBase) _serviceProvider.DefaultIfNull().GetService(type);
            if (viewModel != null)
            {
                viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Initializing, request, metadata);
                viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Initialized, request, metadata);
            }

            return viewModel;
        }

        #endregion
    }
}