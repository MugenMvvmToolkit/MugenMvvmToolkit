using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Members
{
    public sealed class ConstantBindingMemberInfo : IBindingMemberInfo
    {
        #region Fields

        private readonly object? _result;

        public static readonly ConstantBindingMemberInfo NullInstance = new ConstantBindingMemberInfo(null, false);
        public static readonly ConstantBindingMemberInfo UnsetInstance = new ConstantBindingMemberInfo(BindingMetadata.UnsetValue, false);
        public static readonly ConstantBindingMemberInfo WritableNullInstance = new ConstantBindingMemberInfo(null, true);

        public static readonly IBindingMemberInfo[] NullInstanceArray = {NullInstance};
        public static readonly IBindingMemberInfo[] UnsetInstanceArray = {UnsetInstance};
        public static readonly IBindingMemberInfo[] WritableNullInstanceArray = {WritableNullInstance};

        #endregion

        #region Constructors

        private ConstantBindingMemberInfo(object? result, bool canWrite)
        {
            _result = result;
            CanWrite = canWrite;
        }

        #endregion

        #region Properties

        public string Name => string.Empty;

        public Type Type => typeof(object);

        public object? Member => null;

        public BindingMemberType MemberType => BindingMemberType.Empty;

        public bool CanRead => true;

        public bool CanWrite { get; }

        public bool CanObserve => false;

        #endregion

        #region Implementation of interfaces

        public object? GetValue(object? target, object?[]? args, IReadOnlyMetadataContext? metadata = null)
        {
            return _result;
        }

        public object? SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (!CanWrite)
                BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
            return _result;
        }

        public object? SetValues(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null)
        {
            return SetValue(target, null, null);
        }

        public IDisposable? TryObserve(object? target, IBindingEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return null;
        }

        #endregion
    }
}