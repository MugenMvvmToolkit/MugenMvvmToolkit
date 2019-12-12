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

namespace MugenMvvm.Presenters.Components
{
    public sealed class ConditionDecoratorPresenterComponent : DecoratorComponentBase<IPresenter, IPresenterComponent>, IHasPriority, IPresenterComponent
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.PreInitializerHigh;

        #endregion

        #region Implementation of interfaces

        public PresenterResult TryShow(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (!Owner.GetComponents<IConditionPresenterComponent>().CanShow(presenter, metadata))
                    continue;

                var result = presenter.TryShow(metadata, cancellationToken);
                if (!result.IsEmpty)
                    return result;
            }

            return default;
        }

        public IReadOnlyList<PresenterResult>? TryClose(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            var components = Components;
            List<PresenterResult>? results = null;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (!Owner.GetComponents<IConditionPresenterComponent>().CanClose(presenter, (IReadOnlyList<PresenterResult>?) results ?? Default.EmptyArray<PresenterResult>(), metadata))
                    continue;

                var operations = presenter.TryClose(metadata, cancellationToken);
                if (operations == null)
                    continue;
                if (results == null)
                    results = new List<PresenterResult>();
                results.AddRange(operations);
            }

            return results;
        }

        public IReadOnlyList<PresenterResult>? TryRestore(IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            var components = Components;
            List<PresenterResult>? results = null;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (!Owner.GetComponents<IConditionPresenterComponent>().CanRestore(presenter, (IReadOnlyList<PresenterResult>?) results ?? Default.EmptyArray<PresenterResult>(), metadata))
                    continue;

                var operations = presenter.TryRestore(metadata, cancellationToken);
                if (operations == null)
                    continue;
                if (results == null)
                    results = new List<PresenterResult>();
                results.AddRange(operations);
            }

            return results;
        }

        #endregion
    }
}