using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;

namespace MugenMvvm.Bindings.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MemberPathMembers
    {
        #region Fields

        private readonly IReadOnlyList<IMemberInfo>? _members;
        private readonly object? _target;

        #endregion

        #region Constructors

        public MemberPathMembers(object? target, IReadOnlyList<IMemberInfo> members)
        {
            Should.NotBeNull(members, nameof(members));
            _target = target;
            _members = members;
        }

        public MemberPathMembers(Exception exception)
        {
            Should.NotBeNull(exception, nameof(exception));
            _target = exception;
            _members = null;
        }

        #endregion

        #region Properties

        public bool IsAvailable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _members != null;
        }

        public Exception? Error
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_members == null)
                    return (Exception?) _target;
                return null;
            }
        }

        public object? Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_members == null)
                    return BindingMetadata.UnsetValue;
                return _target;
            }
        }

        public IReadOnlyList<IMemberInfo> Members
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _members ?? ConstantMemberInfo.UnsetArray;
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ThrowIfError()
        {
            if (_members == null)
            {
                if (_target is Exception e)
                    throw e;
                return false;
            }

            return true;
        }

        #endregion
    }
}