using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public class BindingManager : ComponentOwnerBase<IBindingManager>, IBindingManager
    {
        #region Constructors

        public BindingManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpression<T>(in T expression, IReadOnlyMetadataContext? metadata = null)
        {
            throw new NotImplementedException();
        }

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> BuildBinding<T>(in T expression, object target, ItemOrList<object?, IReadOnlyList<object?>> sources = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            throw new NotImplementedException();
        }

        public ItemOrList<IBinding?, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyMetadataContext OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}