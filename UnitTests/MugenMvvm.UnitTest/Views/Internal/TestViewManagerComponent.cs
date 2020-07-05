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

        public Func<IViewManager, IViewMapping, object, Type, IReadOnlyMetadataContext?, CancellationToken, Task<IView>?>? TryInitializeAsync { get; set; }

        public Func<IViewManager, IView, object?, Type, IReadOnlyMetadataContext?, CancellationToken, Task?>? TryCleanupAsync { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Task<IView>? IViewManagerComponent.TryInitializeAsync<TRequest>(IViewManager viewManager, IViewMapping mapping, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryInitializeAsync?.Invoke(viewManager, mapping, request!, typeof(TRequest), metadata, cancellationToken);
        }

        Task? IViewManagerComponent.TryCleanupAsync<TRequest>(IViewManager viewManager, IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryCleanupAsync?.Invoke(viewManager, view, request, typeof(TRequest), metadata, cancellationToken);
        }

        #endregion
    }
}