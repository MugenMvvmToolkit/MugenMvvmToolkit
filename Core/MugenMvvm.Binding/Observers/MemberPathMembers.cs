using System;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct MemberPathMembers
    {
        #region Fields

        private readonly IMemberPath _path;
        private readonly IBindingMemberInfo? _lastMember;
        private readonly IBindingMemberInfo[]? _members;
        private readonly object? _penultimateValue;
        private readonly object? _source;

        #endregion

        #region Constructors

        public MemberPathMembers(IMemberPath path)
        {
            Should.NotBeNull(path, nameof(path));
            _path = path;
            _source = null;
            _penultimateValue = null;
            _members = null;
            _lastMember = null;
        }

        public MemberPathMembers(IMemberPath path, object? source, object? penultimateValue, IBindingMemberInfo[]? members, IBindingMemberInfo lastMember)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(lastMember, nameof(lastMember));
            _path = path;
            _source = source;
            _penultimateValue = penultimateValue;
            _members = members;
            _lastMember = lastMember;
        }

        public MemberPathMembers(IMemberPath path, Exception exception)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(exception, nameof(exception));
            _path = path;
            _source = null;
            _penultimateValue = exception;
            _members = null;
            _lastMember = null;
        }

        #endregion

        #region Properties

        public bool IsAvailable => _lastMember != null;

        public IMemberPath Path => _path ?? EmptyMemberPath.Instance;

        public Exception? Error
        {
            get
            {
                if (_lastMember == null)
                    return (Exception?) _penultimateValue;
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

        public object? PenultimateValue
        {
            get
            {
                if (_lastMember == null)
                    return BindingMetadata.UnsetValue;
                return _penultimateValue;
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

        public IBindingMemberInfo[] Members
        {
            get
            {
                if (_lastMember == null)
                    return ConstantBindingMemberInfo.UnsetArray;
                if (_members == null)
                    return new[] {_lastMember};
                return _members;
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
            return ((IBindingPropertyInfo) LastMember).GetValue(PenultimateValue, metadata);
        }

        public void SetLastMemberValue(object? value, IReadOnlyMetadataContext? metadata = null)
        {
            ((IBindingPropertyInfo) LastMember).SetValue(PenultimateValue, value, metadata);
        }

        #endregion
    }
}