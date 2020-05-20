using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Presenters;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestPresenterComponent : IPresenterComponent, IHasPriority
    {
        #region Properties

        public Func<object, Type, IReadOnlyMetadataContext?, CancellationToken, IPresenterResult?>? TryShow { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, CancellationToken, IReadOnlyList<IPresenterResult>>? TryClose { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, CancellationToken, IReadOnlyList<IPresenterResult>>? TryRestore { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IPresenterResult? IPresenterComponent.TryShow<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            return TryShow?.Invoke(request!, typeof(TRequest), metadata, cancellationToken) ?? default;
        }

        IReadOnlyList<IPresenterResult>? IPresenterComponent.TryClose<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            return TryClose?.Invoke(request!, typeof(TRequest), metadata, cancellationToken);
        }

        IReadOnlyList<IPresenterResult>? IPresenterComponent.TryRestore<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            return TryRestore?.Invoke(request!, typeof(TRequest), metadata, cancellationToken);
        }

        #endregion
    }
}