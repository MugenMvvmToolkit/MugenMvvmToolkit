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

        private readonly IMemberInfo[]? _members;
        private readonly object? _target;

        #endregion

        #region Constructors

        public MemberPathMembers(object? target, IMemberInfo[]? members)
        {
            _target = target;
            _members = members;
        }

        public MemberPathMembers(Exception exception)
        {
            _target = exception;
            _target = null;
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
                    return (Exception?) _target;
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

        public IMemberInfo[] Members => _members ?? ConstantMemberInfo.UnsetArray;

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