using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ConditionDecoratorPresenterComponent : DecoratorComponentBase<IPresenter, IPresenterComponent>, IHasPriority,
        IPresenterComponent, ICloseablePresenterComponent, IRestorablePresenterComponent
    {
        #region Fields

        private ICloseablePresenterComponent[] _closeableComponents;
        private ICloseablePresenterComponent[] _closeableDecoratorComponents;
        private IRestorablePresenterComponent[] _restorableComponents;
        private IRestorablePresenterComponent[] _restorableDecoratorComponents;

        #endregion

        #region Constructors

        public ConditionDecoratorPresenterComponent()
        {
            _closeableComponents = Default.EmptyArray<ICloseablePresenterComponent>();
            _closeableDecoratorComponents = _closeableComponents;
            _restorableComponents = Default.EmptyArray<IRestorablePresenterComponent>();
            _restorableDecoratorComponents = _restorableComponents;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.DecoratorHigh;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IPresenterResult> TryClose(IMetadataContext metadata)
        {
            var components = _closeableComponents;
            var results = new List<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (!CanClose(presenter, results, metadata))
                    continue;

                var operations = presenter.TryClose(metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        public IPresenterResult? TryShow(IMetadataContext metadata)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (!CanShow(presenter, metadata))
                    continue;

                var result = presenter.TryShow(metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public IReadOnlyList<IPresenterResult> TryRestore(IMetadataContext metadata)
        {
            var components = _restorableComponents;
            var results = new List<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (!CanRestore(presenter, results, metadata))
                    continue;

                var operations = presenter.TryRestore(metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorInitialize(this, owner, metadata, ref _closeableDecoratorComponents, ref _closeableComponents);
            MugenExtensions.ComponentDecoratorInitialize(this, owner, metadata, ref _restorableDecoratorComponents, ref _restorableComponents);
            base.OnAttachedInternal(owner, metadata);
        }

        protected override void OnDetachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            _closeableComponents = Default.EmptyArray<ICloseablePresenterComponent>();
            _closeableDecoratorComponents = _closeableComponents;
            _restorableComponents = Default.EmptyArray<IRestorablePresenterComponent>();
            _restorableDecoratorComponents = _restorableComponents;
        }

        protected override void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorOnAdded(this, collection, component, ref _closeableDecoratorComponents, ref _closeableComponents);
            MugenExtensions.ComponentDecoratorOnAdded(this, collection, component, ref _restorableDecoratorComponents, ref _restorableComponents);
            base.OnComponentAdded(collection, component, metadata);
        }

        protected override void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorOnRemoved(this, component, ref _closeableDecoratorComponents, ref _closeableComponents);
            MugenExtensions.ComponentDecoratorOnRemoved(this, component, ref _restorableDecoratorComponents, ref _restorableComponents);
            base.OnComponentRemoved(collection, component, metadata);
        }

        private bool CanShow(IPresenterComponent component, IMetadataContext metadata)
        {
            var components = Owner.GetComponents<IConditionPresenterComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanShow(component, metadata))
                    return false;
            }

            return true;
        }

        private bool CanClose(ICloseablePresenterComponent component, IReadOnlyList<IPresenterResult> results,
            IMetadataContext metadata)
        {
            var components = Owner.GetComponents<IConditionPresenterComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanClose(component, results, metadata))
                    return false;
            }

            return true;
        }

        private bool CanRestore(IRestorablePresenterComponent component, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            var components = Owner.GetComponents<IConditionPresenterComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanRestore(component, results, metadata))
                    return false;
            }

            return true;
        }

        #endregion
    }
}