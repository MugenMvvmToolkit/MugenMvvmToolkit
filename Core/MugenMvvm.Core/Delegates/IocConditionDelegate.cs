using System;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Delegates
{
    public delegate bool IocConditionDelegate(IIocContainer container, Type service, object? memberInfo, IReadOnlyMetadataContext? bindingMetadata,
        IReadOnlyMetadataContext? metadata);
}