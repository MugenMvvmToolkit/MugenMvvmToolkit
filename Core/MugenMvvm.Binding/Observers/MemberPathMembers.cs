﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;

namespace MugenMvvm.Binding.Observers
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

        public bool IsAvailable => _members != null;

        public Exception? Error
        {
            get
            {
                if (_members == null)
                    return (Exception?)_target;
                return null;
            }
        }

        public object? Target
        {
            get
            {
                if (_members == null)
                    return BindingMetadata.UnsetValue;
                return _target;
            }
        }

        public IReadOnlyList<IMemberInfo> Members => _members ?? ConstantMemberInfo.UnsetArray;

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfError()
        {
            if (_target is Exception exception)
                throw exception;
        }

        #endregion
    }
}