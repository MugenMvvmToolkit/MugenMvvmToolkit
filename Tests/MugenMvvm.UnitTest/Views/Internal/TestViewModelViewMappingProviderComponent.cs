using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewModelViewMappingProviderComponent : IViewModelViewMappingProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, Type, IReadOnlyMetadataContext?, IReadOnlyList<IViewModelViewMapping>?>? TryGetMappings { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<IViewModelViewMapping>? IViewModelViewMappingProviderComponent.TryGetMappings<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMappings?.Invoke(request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}