using System;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Resources
{
    public class TestTypeResolverComponent : ITypeResolverComponent, IHasPriority
    {
        #region Properties

        public Func<string, object?, Type, IReadOnlyMetadataContext?, Type?>? TryGetType { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Type? ITypeResolverComponent.TryGetType<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetType?.Invoke(name, request, typeof(TRequest), metadata);
        }

        #endregion
    }
}