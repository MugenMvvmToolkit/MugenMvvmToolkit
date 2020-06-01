using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestPresenterComponent : IPresenterComponent, IHasPriority
    {
        #region Properties

        public Func<object, Type, IReadOnlyMetadataContext?, CancellationToken, IReadOnlyList<IPresenterResult>>? TryClose { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, CancellationToken, IReadOnlyList<IPresenterResult>>? TryShow { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<IPresenterResult>? IPresenterComponent.TryShow<TRequest>(in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryShow?.Invoke(request!, typeof(TRequest), metadata, cancellationToken);
        }

        IReadOnlyList<IPresenterResult>? IPresenterComponent.TryClose<TRequest>(in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryClose?.Invoke(request!, typeof(TRequest), metadata, cancellationToken);
        }

        #endregion
    }
}