using System;

namespace MugenMvvm.Bindings.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
    public sealed class NonObservableAttribute : Attribute
    {
    }
}