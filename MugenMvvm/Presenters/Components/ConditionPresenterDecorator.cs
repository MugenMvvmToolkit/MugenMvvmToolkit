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

        public int Priority { get; set; } = PresenterComponentPriority.ConditionDecorator;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var components = Components;
            var result = ItemOrListEditor.Get<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
            {
                var presenterComponent = components[i];
                if (Owner.GetComponents<IConditionPresenterComponent>().CanShow(presenter, presenterComponent, result.ToItemOrList<IReadOnlyList<IPresenterResult>>(), request, metadata))
                    result.AddRange(presenterComponent.TryShow(presenter, request, cancellationToken, metadata));
            }

            return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var components = Components;
            var result = ItemOrListEditor.Get<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
            {
                var presenterComponent = components[i];
                if (Owner.GetComponents<IConditionPresenterComponent>().CanClose(presenter, presenterComponent, result.ToItemOrList<IReadOnlyList<IPresenterResult>>(), request, metadata))
                    result.AddRange(presenterComponent.TryClose(presenter, request, cancellationToken, metadata));
            }

            return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
        }

        #endregion
    }
}