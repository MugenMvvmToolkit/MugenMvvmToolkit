using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MemberPathLastMember
    {
        private readonly IMemberInfo? _member;
        private readonly object? _target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemberPathLastMember(object? target, IMemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            _target = target;
            _member = member;
        }

        public MemberPathLastMember(Exception exception)
        {
            Should.NotBeNull(exception, nameof(exception));
            _target = exception;
            _member = null;
        }

        public bool IsAvailable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _member != null;
        }

        public Exception? Error
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_member == null)
                    return (Exception?) _target;
                return null;
            }
        }

        public object? Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_member == null)
                    return BindingMetadata.UnsetValue;
                return _target;
            }
        }

        public IMemberInfo Member
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_member == null)
                    return ConstantMemberInfo.Unset;
                return _member;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ThrowIfError()
        {
            if (_member == null)
            {
                if (_target is Exception e)
                    ExceptionManager.Throw(e);
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? GetValueOrThrow(IReadOnlyMetadataContext? metadata = null)
        {
            if (_member == null)
            {
                ThrowIfError();
                return BindingMetadata.UnsetValue;
            }

            return ((IAccessorMemberInfo) _member).GetValue(_target, metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? GetValue(IReadOnlyMetadataContext? metadata = null) => ((IAccessorMemberInfo) _member!).GetValue(_target, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrySetValueWithConvert(object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (_member is IAccessorMemberInfo member)
                member.SetValue(_target, MugenService.GlobalValueConverter.Convert(value, member.Type, member, metadata), metadata);
        }
    }
}