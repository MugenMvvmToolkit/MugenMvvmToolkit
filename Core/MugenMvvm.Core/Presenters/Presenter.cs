using System;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;

namespace MugenMvvm.Presenters
{
    public class Presenter : ComponentOwnerBase<IPresenter>, IPresenter
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

        #region Properties

        protected IMetadataContextProvider MetadataContextProvider => _metadataContextProvider.ServiceIfNull();

        #endregion

        #region Implementation of interfaces

        public IPresenterResult Show(IReadOnlyMetadataContext metadata)
        {
            var operationId = Default.NextCounter().ToString();
            var metadataContext = MetadataContextProvider.GetMetadataContext(this, metadata);

            try
            {
                var result = ShowInternal(operationId, metadataContext);
                if (result == null)
                    ExceptionManager.ThrowPresenterCannotShowRequest(metadata);

                return OnShownInternal(operationId, result!, metadataContext);
            }
            catch (Exception e)
            {
                OnShowError(operationId, e, metadataContext);
                throw;
            }
        }

        public IReadOnlyList<IPresenterResult> TryClose(IReadOnlyMetadataContext metadata)
        {
            var operationId = Default.NextCounter().ToString();
            var metadataContext = MetadataContextProvider.GetMetadataContext(this, metadata);

            try
            {
                var result = TryCloseInternal(operationId, metadataContext);
                return OnClosedInternal(operationId, result, metadataContext);
            }
            catch (Exception e)
            {
                OnCloseError(operationId, e, metadataContext);
                throw;
            }
        }

        public IReadOnlyList<IPresenterResult> TryRestore(IReadOnlyMetadataContext metadata)
        {
            var operationId = Default.NextCounter().ToString();
            var metadataContext = MetadataContextProvider.GetMetadataContext(this, metadata);

            try
            {
                var result = TryRestoreInternal(operationId, metadataContext);
                return OnRestoredInternal(operationId, result, metadataContext);
            }
            catch (Exception e)
            {
                OnRestoreError(operationId, e, metadataContext);
                throw;
            }
        }

        #endregion

        #region Methods

        protected virtual IPresenterResult? ShowInternal(string operationId, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterShowListener)?.OnShowing(this, operationId, metadata);

            for (var i = 0; i < components.Length; i++)
            {
                if (!(components[i] is IPresenterComponent presenter))
                    continue;

                if (!CanShow(presenter, metadata))
                    continue;

                var operation = presenter.TryShow(metadata);
                if (operation != null)
                    return operation;
            }

            return null;
        }

        protected virtual IPresenterResult OnShownInternal(string operationId, IPresenterResult result, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterShowListener)?.OnShown(this, operationId, result, metadata);

            return result;
        }

        protected virtual bool CanShow(IPresenterComponent presenter, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent component && !component.CanShow(presenter, metadata))
                    return false;
            }

            return true;
        }

        protected virtual void OnShowError(string operationId, Exception exception, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterShowListener)?.OnShowError(this, operationId, exception, metadata);
        }

        protected virtual IReadOnlyList<IPresenterResult> TryCloseInternal(string operationId, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterCloseListener)?.OnClosing(this, operationId, metadata);

            var results = new List<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
            {
                if (!(components[i] is ICloseablePresenterComponent presenter))
                    continue;

                if (!CanClose(presenter, results, metadata))
                    continue;

                var operations = presenter.TryClose(metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        protected virtual IReadOnlyList<IPresenterResult> OnClosedInternal(string operationId, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterCloseListener)?.OnClosed(this, operationId, results, metadata);
            return results;
        }

        protected virtual bool CanClose(ICloseablePresenterComponent presenter, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent component && !component.CanClose(presenter, results, metadata))
                    return false;
            }

            return true;
        }

        protected virtual void OnCloseError(string operationId, Exception exception, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterCloseListener)?.OnCloseError(this, operationId, exception, metadata);
        }

        protected virtual IReadOnlyList<IPresenterResult> TryRestoreInternal(string operationId, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterRestoreListener)?.OnRestoring(this, operationId, metadata);

            var results = new List<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
            {
                if (!(components[i] is IRestorablePresenterComponent presenter))
                    continue;

                if (!CanRestore(presenter, results, metadata))
                    continue;

                var operations = presenter.TryRestore(metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        protected virtual IReadOnlyList<IPresenterResult> OnRestoredInternal(string operationId, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterRestoreListener)?.OnRestored(this, operationId, results, metadata);
            return results;
        }

        protected virtual bool CanRestore(IRestorablePresenterComponent presenter, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionPresenterComponent component && !component.CanRestore(presenter, results, metadata))
                    return false;
            }

            return true;
        }

        protected virtual void OnRestoreError(string operationId, Exception exception, IMetadataContext metadata)
        {
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IPresenterRestoreListener)?.OnRestoreError(this, operationId, exception, metadata);
        }

        #endregion
    }
}