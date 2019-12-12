﻿using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Presenters;

namespace MugenMvvm.Extensions.Components
{
    public static class PresenterComponentExtensions
    {
        #region Methods

        public static bool CanShow(this IConditionPresenterComponent[] components, IPresenterComponent component, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanShow(component, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanClose(this IConditionPresenterComponent[] components, IPresenterComponent component, IReadOnlyList<PresenterResult> results, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanClose(component, results, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanRestore(this IConditionPresenterComponent[] components, IPresenterComponent component, IReadOnlyList<PresenterResult> results, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanRestore(component, results, metadata))
                    return false;
            }

            return true;
        }

        public static PresenterResult TryShow(this IPresenterComponent[] components, IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryShow(metadata, cancellationToken);
                if (!result.IsEmpty)
                    return result;
            }

            return default;
        }

        public static IReadOnlyList<PresenterResult>? TryClose(this IPresenterComponent[] components, IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            List<PresenterResult>? results = null;
            for (var i = 0; i < components.Length; i++)
            {
                var operations = components[i].TryClose(metadata, cancellationToken);
                if (operations == null || operations.Count == 0)
                    continue;
                if (results == null)
                    results = new List<PresenterResult>();
                results.AddRange(operations);
            }

            return results;
        }

        public static IReadOnlyList<PresenterResult>? TryRestore(this IPresenterComponent[] components, IReadOnlyMetadataContext metadata, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            List<PresenterResult>? results = null;
            for (var i = 0; i < components.Length; i++)
            {
                var operations = components[i].TryRestore(metadata, cancellationToken);
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