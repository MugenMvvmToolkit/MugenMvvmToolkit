using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MemberPathLastMember
    {
        #region Fields

        private readonly IBindingMemberInfo? _lastMember;
        private readonly object? _target;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemberPathLastMember(object? target, IBindingMemberInfo lastMember)
        {
            _target = target;
            _lastMember = lastMember;
        }

        public MemberPathLastMember(Exception exception)
        {
            _target = exception;
            _lastMember = null;
        }

        #endregion

        #region Properties

        public bool IsAvailable => _lastMember != null;

        public Exception? Error
        {
            get
            {
                if (_lastMember == null)
                    return (Exception?)_target;
                return null;
            }
        }

        public object? Target
        {
            get
            {
                if (_lastMember == null)
                    return BindingMetadata.UnsetValue;
                return _target;
            }
        }

        public IBindingMemberInfo LastMember
        {
            get
            {
                if (_lastMember == null)
                    return ConstantBindingMemberInfo.Unset;
                return _lastMember;
            }
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfError()
        {
            if (_lastMember == null && _target is Exception e)
                throw e;
        }

        public object? GetValue(IReadOnlyMetadataContext? metadata = null)
        {
            return ((IBindingMemberAccessorInfo)_lastMember!).GetValue(_target, metadata);
        }

        public void SetValue(object? value, IReadOnlyMetadataContext? metadata = null)
        {
            ((IBindingMemberAccessorInfo)_lastMember!).SetValue(_target, value, metadata);
        }

        #endregion
    }
}