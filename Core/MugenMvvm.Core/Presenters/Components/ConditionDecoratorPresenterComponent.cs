using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ConditionDecoratorPresenterComponent : DecoratorComponentBase<IPresenter, IPresenterComponent>,
        IHasPriority, IPresenterComponent, ICloseablePresenterComponent, IRestorablePresenterComponent,
        IDecoratorComponentCollectionComponent<ICloseablePresenterComponent>, IDecoratorComponentCollectionComponent<IRestorablePresenterComponent>
    {
        #region Fields

        private ICloseablePresenterComponent[] _closeableComponents;
        private IRestorablePresenterComponent[] _restorableComponents;

        #endregion

        #region Constructors

        public ConditionDecoratorPresenterComponent()
        {
            _closeableComponents = Default.EmptyArray<ICloseablePresenterComponent>();
            _restorableComponents = Default.EmptyArray<IRestorablePresenterComponent>();
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
                if (!Owner.GetComponents<IConditionPresenterComponent>().CanClose(presenter, results, metadata))
                    continue;

                var operations = presenter.TryClose(metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        void IDecoratorComponentCollectionComponent<ICloseablePresenterComponent>.Decorate(IList<ICloseablePresenterComponent> components, IReadOnlyMetadataContext? metadata)
        {
            this.Decorate(Owner, components, this, ref _closeableComponents);
        }

        void IDecoratorComponentCollectionComponent<IRestorablePresenterComponent>.Decorate(IList<IRestorablePresenterComponent> components, IReadOnlyMetadataContext? metadata)
        {
            this.Decorate(Owner, components, this, ref _restorableComponents);
        }

        public IPresenterResult? TryShow(IMetadataContext metadata)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                var presenter = components[i];
                if (!Owner.GetComponents<IConditionPresenterComponent>().CanShow(presenter, metadata))
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
                if (!Owner.GetComponents<IConditionPresenterComponent>().CanRestore(presenter, results, metadata))
                    continue;

                var operations = presenter.TryRestore(metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        #endregion

        #region Methods

        protected override void OnDetachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            _closeableComponents = Default.EmptyArray<ICloseablePresenterComponent>();
            _restorableComponents = Default.EmptyArray<IRestorablePresenterComponent>();
        }

        #endregion
    }
}