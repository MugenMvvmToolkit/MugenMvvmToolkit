using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Presenters;

namespace MugenMvvm.Extensions.Components
{
    public static class PresenterComponentExtensions
    {
        #region Methods

        public static bool CanShow<TRequest>(this IConditionPresenterComponent[] components, IPresenterComponent presenter, in TRequest request, IReadOnlyMetadataContext? metadata)
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

        public static bool CanClose<TRequest>(this IConditionPresenterComponent[] components, IPresenterComponent presenter, IReadOnlyList<PresenterResult> results, in TRequest request,
            IReadOnlyMetadataContext? metadata)
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

        public static bool CanRestore<TRequest>(this IConditionPresenterComponent[] components, IPresenterComponent presenter, IReadOnlyList<PresenterResult> results, in TRequest request,
            IReadOnlyMetadataContext? metadata)
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

        public static PresenterResult TryShow<TRequest>(this IPresenterComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryShow(request, metadata, cancellationToken);
                if (!result.IsEmpty)
                    return result;
            }

            return default;
        }

        public static IReadOnlyList<PresenterResult>? TryClose<TRequest>(this IPresenterComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            List<PresenterResult>? results = null;
            for (var i = 0; i < components.Length; i++)
            {
                var operations = components[i].TryClose(request, metadata, cancellationToken);
                if (operations == null || operations.Count == 0)
                    continue;
                if (results == null)
                    results = new List<PresenterResult>();
                results.AddRange(operations);
            }

            return results;
        }

        public static IReadOnlyList<PresenterResult>? TryRestore<TRequest>(this IPresenterComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            List<PresenterResult>? results = null;
            for (var i = 0; i < components.Length; i++)
            {
                var operations = components[i].TryRestore(request, metadata, cancellationToken);
                if (operations == null || operations.Count == 0)
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