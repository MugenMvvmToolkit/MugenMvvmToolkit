using System;
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
        private readonly object? _source;

        #endregion

        #region Constructors

        public MemberPathLastMember(object? source, IBindingMemberInfo lastMember)
        {
            Should.NotBeNull(lastMember, nameof(lastMember));
            _source = source;
            _lastMember = lastMember;
        }

        public MemberPathLastMember(Exception exception)
        {
            Should.NotBeNull(exception, nameof(exception));
            _source = exception;
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
                    return (Exception?) _source;
                return null;
            }
        }

        public object? Source
        {
            get
            {
                if (_lastMember == null)
                    return BindingMetadata.UnsetValue;
                return _source;
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

        public void ThrowIfError()
        {
            if (Error != null)
                throw Error!;
        }

        public object? GetLastMemberValue(IReadOnlyMetadataContext? metadata = null)
        {
            return ((IBindingPropertyInfo) LastMember).GetValue(Source, metadata);
        }

        public void SetLastMemberValue(object? value, IReadOnlyMetadataContext? metadata = null)
        {
            ((IBindingPropertyInfo) LastMember).SetValue(Source, value, metadata);
        }

        #endregion
    }
}