using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public abstract class ComponentOwnerBase<T> : IComponentOwner<T> where T : class
    {
        #region Fields

        private readonly IComponentCollectionManager? _componentCollectionManager;
        private IComponentCollection? _components;

        #endregion

        #region Constructors

        protected ComponentOwnerBase(IComponentCollectionManager? componentCollectionManager)
        {
            _componentCollectionManager = componentCollectionManager;
        }

        #endregion

        #region Properties

        protected IComponentCollectionManager ComponentCollectionManager => _componentCollectionManager.DefaultIfNull();

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components => _components ?? ComponentCollectionManager.EnsureInitialized(ref _components, this);

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ItemOrArray<TComponent> GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class
        {
            if (_components == null)
                return default;
            return _components.Get<TComponent>(metadata);
        }

        #endregion
    }
}