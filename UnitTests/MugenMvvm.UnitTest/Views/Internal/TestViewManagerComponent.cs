using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewManagerComponent : IViewManagerComponent, IHasPriority
    {
        #region Properties

        public Func<IViewMapping, object, Type, IReadOnlyMetadataContext?, CancellationToken, Task<IView>?>? TryInitializeAsync { get; set; }

        public Func<IView, object?, Type, IReadOnlyMetadataContext?, CancellationToken, Task?>? TryCleanupAsync { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Task<IView>? IViewManagerComponent.TryInitializeAsync<TRequest>(IViewMapping mapping, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryInitializeAsync?.Invoke(mapping, request!, typeof(TRequest), metadata, cancellationToken);
        }

        Task? IViewManagerComponent.TryCleanupAsync<TRequest>(IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryCleanupAsync?.Invoke(view, request, typeof(TRequest), metadata, cancellationToken);
        }

        #endregion
    }
}