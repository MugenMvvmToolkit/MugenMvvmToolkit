using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ConditionPresenterDecorator : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IHasPriority, IPresenterComponent
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        public PresenterResult TryShow<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (!Owner.GetComponents<IConditionPresenterComponent>().CanShow(presenter, request, metadata))
                    continue;

                var result = presenter.TryShow(request, metadata, cancellationToken);
                if (!result.IsEmpty)
                    return result;
            }

            return default;
        }

        public IReadOnlyList<PresenterResult>? TryClose<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            var components = Components;
            LazyList<PresenterResult> result = default;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (Owner.GetComponents<IConditionPresenterComponent>().CanClose(presenter, (IReadOnlyList<PresenterResult>?)result.List ?? Default.EmptyArray<PresenterResult>(), request, metadata))
                    result.AddRange(presenter.TryClose(request, metadata, cancellationToken));
            }

            return result.List;
        }

        public IReadOnlyList<PresenterResult>? TryRestore<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            var components = Components;
            LazyList<PresenterResult> result = default;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (Owner.GetComponents<IConditionPresenterComponent>().CanRestore(presenter, (IReadOnlyList<PresenterResult>?)result.List ?? Default.EmptyArray<PresenterResult>(), request, metadata))
                    result.AddRange(presenter.TryRestore(request, metadata, cancellationToken));
            }

            return result.List;
        }

        #endregion
    }
}