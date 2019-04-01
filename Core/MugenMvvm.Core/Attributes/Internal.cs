using System;
// ReSharper disable CheckNamespace

#if !NET40
namespace System
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate,
        Inherited = false)]
    internal sealed class SerializableAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class NonSerializedAttribute : Attribute
    {
    }
}
#endif

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class AssertsTrueAttribute : Attribute
    {
        public AssertsTrueAttribute () { }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class AssertsFalseAttribute : Attribute
    {
        public AssertsFalseAttribute () { }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class EnsuresNotNullAttribute : Attribute
    {
        public EnsuresNotNullAttribute() { }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class NotNullWhenFalseAttribute : Attribute
    {
        public NotNullWhenFalseAttribute() { }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal class NotNullWhenTrueAttribute : Attribute
    {
        public NotNullWhenTrueAttribute() { }
    }
}

namespace MugenMvvm.Attributes
{
    [AttributeUsage(
        AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method |
        AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    internal sealed class PreserveAttribute : Attribute
    {
        #region Fields

        public bool AllMembers;

        public bool Conditional;

        #endregion
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class LinkerSafeAttribute : Attribute
    {
    }
}