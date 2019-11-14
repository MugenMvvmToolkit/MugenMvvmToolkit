using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class ConstantMemberInfo : IMemberAccessorInfo
    {
        #region Fields

        private readonly object? _result;

        public static readonly ConstantMemberInfo Null = new ConstantMemberInfo(null, false);
        public static readonly ConstantMemberInfo Unset = new ConstantMemberInfo(BindingMetadata.UnsetValue, false);
        public static readonly ConstantMemberInfo WritableNull = new ConstantMemberInfo(null, true);

        public static readonly IMemberInfo[] NullArray = { Null };
        public static readonly IMemberInfo[] UnsetArray = { Unset };
        public static readonly IMemberInfo[] WritableNullArray = { WritableNull };

        #endregion

        #region Constructors

        private ConstantMemberInfo(object? result, bool canWrite)
        {
            _result = result;
            CanWrite = canWrite;
        }

        #endregion

        #region Properties

        public string Name => string.Empty;

        public Type DeclaringType => typeof(object);

        public Type Type => typeof(object);

        public object? UnderlyingMember => null;

        public MemberType MemberType => MemberType.Property;

        public MemberFlags AccessModifiers => MemberFlags.StaticPublic;

        public bool CanRead => true;

        public bool CanWrite { get; }

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return default;
        }

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            return _result;
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (!CanWrite)
                BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
        }

        #endregion
    }
}