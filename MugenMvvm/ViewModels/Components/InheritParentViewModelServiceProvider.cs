using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class InheritParentViewModelServiceProvider : IViewModelServiceProviderComponent, IHasPriority
    {
        public readonly Dictionary<Type, Func<IViewModelBase, object?>> ServiceMapping;

        public InheritParentViewModelServiceProvider()
        {
            ServiceMapping = new Dictionary<Type, Func<IViewModelBase, object?>>(3, InternalEqualityComparer.Type);
        }

        public int Priority { get; set; } = ViewModelComponentPriority.InheritParentServiceResolver;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetService<T>(object vm) where T : class => (vm as IHasService<T>)?.GetService(false);

        public object? TryGetService(IViewModelManager viewModelManager, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is not Type t || !ServiceMapping.TryGetValue(t, out var handler) || !viewModel.Metadata.TryGet(ViewModelMetadata.ParentViewModel, out var parent) &&
                !metadata.DefaultIfNull().TryGet(ViewModelMetadata.ParentViewModel, out parent) || parent == null)
                return null;
            return handler(parent);
        }
    }
}