﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewInitializerComponent : IViewInitializerComponent, IHasPriority
    {
        #region Properties

        public Func<IViewModelViewMapping, object, Type, IReadOnlyMetadataContext?, CancellationToken, Task<IView>?>? TryInitializeAsync { get; set; }

        public Func<IView, object?, Type, IReadOnlyMetadataContext?, CancellationToken, Task?>? TryCleanupAsync { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Task<IView>? IViewInitializerComponent.TryInitializeAsync<TRequest>(IViewModelViewMapping mapping, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryInitializeAsync?.Invoke(mapping, request!, typeof(TRequest), metadata, cancellationToken);
        }

        Task? IViewInitializerComponent.TryCleanupAsync<TRequest>(IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryCleanupAsync?.Invoke(view, request, typeof(TRequest), metadata, cancellationToken);
        }

        #endregion
    }
}