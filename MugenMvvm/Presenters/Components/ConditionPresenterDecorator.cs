using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var components = Components;
            ItemOrList<IPresenterResult, List<IPresenterResult>> result = default;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (Owner.GetComponents<IConditionPresenterComponent>().CanShow(presenter, result.Cast<IReadOnlyList<IPresenterResult>>(), request, metadata))
                    result.AddRange(presenter.TryShow(request, cancellationToken, metadata));
            }

            return result.Cast<IReadOnlyList<IPresenterResult>>();
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var components = Components;
            ItemOrList<IPresenterResult, List<IPresenterResult>> result = default;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (Owner.GetComponents<IConditionPresenterComponent>().CanClose(presenter, result.Cast<IReadOnlyList<IPresenterResult>>(), request, metadata))
                    result.AddRange(presenter.TryClose(request, cancellationToken, metadata));
            }

            return result.Cast<IReadOnlyList<IPresenterResult>>();
        }

        #endregion
    }
}