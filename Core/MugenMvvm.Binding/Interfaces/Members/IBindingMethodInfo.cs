using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingMethodInfo : IObservableBindingMemberInfo
    {
        bool IsExtensionMethod { get; }

        bool IsGenericMethod { get; }

        bool IsGenericMethodDefinition { get; }

        IBindingParameterInfo[] GetParameters();

        Type[] GetGenericArguments();

        IBindingMethodInfo MakeGenericMethod(Type[] types);

        object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null);
    }
}