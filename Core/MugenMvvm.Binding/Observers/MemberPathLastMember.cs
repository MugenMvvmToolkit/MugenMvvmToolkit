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

        private readonly IMemberInfo? _member;
        private readonly object? _target;

        #endregion

        #region Constructors

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

        #endregion

        #region Properties

        public bool IsAvailable => _member != null;

        public Exception? Error
        {
            get
            {
                if (_member == null)
                    return (Exception?)_target;
                return null;
            }
        }

        public object? Target
        {
            get
            {
                if (_member == null)
                    return BindingMetadata.UnsetValue;
                return _target;
            }
        }

        public IMemberInfo Member
        {
            get
            {
                if (_member == null)
                    return ConstantMemberInfo.Unset;
                return _member;
            }
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfError()
        {
            if (_member == null && _target is Exception e)
                throw e;
        }

        public object? GetValueOrThrow(IReadOnlyMetadataContext? metadata = null)
        {
            if (_member == null)
            {
                ThrowIfError();
                return BindingMetadata.UnsetValue;
            }
            return ((IMemberAccessorInfo)_member).GetValue(_target, metadata);
        }

        public object? GetValue(IReadOnlyMetadataContext? metadata = null)
        {
            return ((IMemberAccessorInfo)_member!).GetValue(_target, metadata);
        }

        public void SetValueWithConvert(object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (_member is IMemberAccessorInfo member)
                member.SetValue(_target, MugenBindingService.GlobalValueConverter.Convert(value, member.Type, member, metadata), metadata);
        }

        #endregion
    }
}