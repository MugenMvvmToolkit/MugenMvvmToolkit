using System;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;

namespace MugenMvvm.Presenters
{
    public sealed class Presenter : ComponentOwnerBase<IPresenter>, IPresenter
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        public Presenter(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Implementation of interfaces

        public IPresenterResult Show(IReadOnlyMetadataContext metadata)
        {
            var operationId = Default.NextCounter().ToString();
            var metadataContext = _metadataContextProvider.ServiceIfNull().GetMetadataContext(this, metadata);
            var components = Components.GetComponents();

            try
            {
                IPresenterResult? result = null;
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterShowListener)?.OnShowing(this, operationId, metadataContext);

                for (var i = 0; i < components.Length; i++)
                {
                    if (!(components[i] is IPresenterComponent presenter))
                        continue;

                    if (!CanShow(components, presenter, metadataContext))
                        continue;

                    result = presenter.TryShow(metadataContext);
                    if (result != null)
                        break;
                }

                if (result == null)
                    ExceptionManager.ThrowPresenterCannotShowRequest(metadata);

                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterShowListener)?.OnShown(this, operationId, result, metadataContext);
                return result;
            }
            catch (Exception e)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterShowListener)?.OnShowError(this, operationId, e, metadataContext);
                throw;
            }
        }

        public IReadOnlyList<IPresenterResult> TryClose(IReadOnlyMetadataContext metadata)
        {
            var operationId = Default.NextCounter().ToString();
            var metadataContext = _metadataContextProvider.ServiceIfNull().GetMetadataContext(this, metadata);
            var components = Components.GetComponents();

            try
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterCloseListener)?.OnClosing(this, operationId, metadataContext);

                var results = new List<IPresenterResult>();
                for (var i = 0; i < components.Length; i++)
                {
                    if (!(components[i] is ICloseablePresenterComponent presenter))
                        continue;

                    if (!CanClose(components, presenter, results, metadataContext))
                        continue;

                    var operations = presenter.TryClose(metadataContext);
                    if (operations != null)
                        results.AddRange(operations);
                }

                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterCloseListener)?.OnClosed(this, operationId, results, metadataContext);
                return results;
            }
            catch (Exception e)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterCloseListener)?.OnCloseError(this, operationId, e, metadataContext);
                throw;
            }
        }

        public IReadOnlyList<IPresenterResult> TryRestore(IReadOnlyMetadataContext metadata)
        {
            var operationId = Default.NextCounter().ToString();
            var metadataContext = _metadataContextProvider.ServiceIfNull().GetMetadataContext(this, metadata);
            var components = Components.GetComponents();

            try
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterRestoreListener)?.OnRestoring(this, operationId, metadataContext);

                var results = new List<IPresenterResult>();
                for (var i = 0; i < components.Length; i++)
                {
                    if (!(components[i] is IRestorablePresenterComponent presenter))
                        continue;

                    if (!CanRestore(components, presenter, results, metadataContext))
                        continue;

                    var operations = presenter.TryRestore(metadataContext);
                    if (operations != null)
                        results.AddRange(operations);
                }

                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterRestoreListener)?.OnRestored(this, operationId, results, metadataContext);
                return results;
            }
            catch (Exception e)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IPresenterRestoreListener)?.OnRestoreError(this, operationId, e, metadataContext);
                throw;
            }
        }

        #endregion

        #region Methods

        private static bool CanShow(IComponent<IPresenter>[] components, IPresenterComponent presenter, IMetadataContext metadata)
        {
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent component && !component.CanShow(presenter, metadata))
                    return false;
            }

            return true;
        }

        private static bool CanClose(IComponent<IPresenter>[] components, ICloseablePresenterComponent presenter, IReadOnlyList<IPresenterResult> results,
            IMetadataContext metadata)
        {
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent component && !component.CanClose(presenter, results, metadata))
                    return false;
            }

            return true;
        }

        private static bool CanRestore(IComponent<IPresenter>[] components, IRestorablePresenterComponent presenter, IReadOnlyList<IPresenterResult> results,
            IMetadataContext metadata)
        {
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent component && !component.CanRestore(presenter, results, metadata))
                    return false;
            }

            return true;
        }

        #endregion
    }
}