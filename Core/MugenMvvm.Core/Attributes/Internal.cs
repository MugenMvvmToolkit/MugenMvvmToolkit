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
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class AssertsTrueAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal class AssertsFalseAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal class EnsuresNotNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal class NotNullWhenFalseAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal class NotNullWhenTrueAttribute : Attribute
    {
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