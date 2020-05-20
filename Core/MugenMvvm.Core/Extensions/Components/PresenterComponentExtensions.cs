using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;
using MugenMvvm.Presenters;

namespace MugenMvvm.Extensions.Components
{
    public static class PresenterComponentExtensions
    {
        #region Methods

        public static bool CanShow<TRequest>(this IConditionPresenterComponent[] components, IPresenterComponent presenter, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanShow(presenter, request, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanClose<TRequest>(this IConditionPresenterComponent[] components, IPresenterComponent presenter, IReadOnlyList<IPresenterResult> results,
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

        public static bool CanRestore<TRequest>(this IConditionPresenterComponent[] components, IPresenterComponent presenter, IReadOnlyList<IPresenterResult> results,
            [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanRestore(presenter, results, request, metadata))
                    return false;
            }

            return true;
        }

        public static IPresenterResult? TryShow<TRequest>(this IPresenterComponent[] components, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryShow(request, metadata, cancellationToken);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IReadOnlyList<IPresenterResult>? TryClose<TRequest>(this IPresenterComponent[] components, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            LazyList<IPresenterResult> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryClose(request, metadata, cancellationToken));
            return result.List;
        }

        public static IReadOnlyList<IPresenterResult>? TryRestore<TRequest>(this IPresenterComponent[] components, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            LazyList<IPresenterResult> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryRestore(request, metadata, cancellationToken));
            return result.List;
        }

        #endregion
    }
}