using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class PresenterComponentExtensions
    {
        #region Methods

        public static bool CanShow<TRequest>(this IConditionPresenterComponent[] components, IPresenterComponent presenter, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results,
            [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanShow(presenter, results, request, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanClose<TRequest>(this IConditionPresenterComponent[] components, IPresenterComponent presenter, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results,
            [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanClose(presenter, results, request, metadata))
                    return false;
            }

            return true;
        }

        public static ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow<TRequest>(this IPresenterComponent[] components, [DisallowNull] in TRequest request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryShow(request, cancellationToken, metadata);
            ItemOrList<IPresenterResult, List<IPresenterResult>> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryShow(request, cancellationToken, metadata));
            return result.Cast<IReadOnlyList<IPresenterResult>>();
        }

        public static ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose<TRequest>(this IPresenterComponent[] components, [DisallowNull] in TRequest request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryClose(request, cancellationToken, metadata);
            ItemOrList<IPresenterResult, List<IPresenterResult>> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryClose(request, cancellationToken, metadata));
            return result.Cast<IReadOnlyList<IPresenterResult>>();
        }

        #endregion
    }
}