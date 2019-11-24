using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static bool CanShow(this IPresenter presenter, IPresenterComponent component, IMetadataContext metadata)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            var components = presenter.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent p && !p.CanShow(component, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanClose(this IPresenter presenter, ICloseablePresenterComponent component, IReadOnlyList<IPresenterResult> results,
            IMetadataContext metadata)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            var components = presenter.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent p && !p.CanClose(component, results, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanRestore(this IPresenter presenter, IRestorablePresenterComponent component, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            var components = presenter.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent p && !p.CanRestore(component, results, metadata))
                    return false;
            }

            return true;
        }

        #endregion
    }
}