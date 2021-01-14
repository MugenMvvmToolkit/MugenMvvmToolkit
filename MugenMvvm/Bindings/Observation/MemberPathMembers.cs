using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;

namespace MugenMvvm.Bindings.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MemberPathMembers
    {
        private readonly object? _membersRaw;
        private readonly object? _target;

        public MemberPathMembers(object? target, ItemOrIReadOnlyList<IMemberInfo> members)
        {
            _target = target;
            _membersRaw = members.GetRawValue();
        }

        public MemberPathMembers(Exception exception)
        {
            Should.NotBeNull(exception, nameof(exception));
            _target = exception;
            _membersRaw = null;
        }

        public bool IsAvailable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _membersRaw != null;
        }

        public Exception? Error
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_membersRaw == null)
                    return (Exception?) _target;
                return null;
            }
        }

        public object? Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_membersRaw == null)
                    return BindingMetadata.UnsetValue;
                return _target;
            }
        }

        public ItemOrIReadOnlyList<IMemberInfo> Members
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(_membersRaw ?? ConstantMemberInfo.Unset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ThrowIfError()
        {
            if (_membersRaw == null)
            {
                if (_target is Exception e)
                    throw e;
                return false;
            }

            return true;
        }
    }
}