using System;
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

        private readonly IBindingMemberInfo[]? _members;
        private readonly object? _source;

        #endregion

        #region Constructors

        public MemberPathMembers(object? source, IBindingMemberInfo[]? members)
        {
            Should.NotBeNull(members, nameof(members));
            _source = source;
            _members = members;
        }

        public MemberPathMembers(Exception exception)
        {
            Should.NotBeNull(exception, nameof(exception));
            _source = exception;
            _source = null;
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
                    return (Exception?) _source;
                return null;
            }
        }

        public object? Source
        {
            get
            {
                if (_members == null)
                    return BindingMetadata.UnsetValue;
                return _source;
            }
        }

        public IBindingMemberInfo[] Members => _members ?? ConstantBindingMemberInfo.UnsetArray;

        #endregion

        #region Methods

        public void ThrowIfError()
        {
            if (Error != null)
                throw Error!;
        }

        #endregion
    }
}