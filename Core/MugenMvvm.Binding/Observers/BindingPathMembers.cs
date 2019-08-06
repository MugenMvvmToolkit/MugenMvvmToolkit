using System;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindingPathMembers
    {
        #region Fields

        private readonly IBindingPath _bindingPath;
        private readonly IBindingMemberInfo? _lastMember;
        private readonly IBindingMemberInfo[]? _members;
        private readonly object? _penultimateValue;
        private readonly object? _source;

        #endregion

        #region Constructors

        public BindingPathMembers(IBindingPath bindingPath)
        {
            Should.NotBeNull(bindingPath, nameof(bindingPath));
            _bindingPath = bindingPath;
            _source = null;
            _penultimateValue = null;
            _members = null;
            _lastMember = null;
        }

        public BindingPathMembers(IBindingPath bindingPath, object? source, object? penultimateValue, IBindingMemberInfo[]? members, IBindingMemberInfo lastMember)
        {
            Should.NotBeNull(bindingPath, nameof(bindingPath));
            Should.NotBeNull(lastMember, nameof(lastMember));
            _bindingPath = bindingPath;
            _source = source;
            _penultimateValue = penultimateValue;
            _members = members;
            _lastMember = lastMember;
        }

        public BindingPathMembers(IBindingPath bindingPath, Exception exception)
        {
            Should.NotBeNull(bindingPath, nameof(bindingPath));
            Should.NotBeNull(exception, nameof(exception));
            _bindingPath = bindingPath;
            _source = null;
            _penultimateValue = exception;
            _members = null;
            _lastMember = null;
        }

        #endregion

        #region Properties

        public bool IsAvailable => _lastMember != null;

        public IBindingPath BindingPath => _bindingPath ?? EmptyBindingPath.Instance;

        public Exception? Error
        {
            get
            {
                if (_lastMember == null)
                    return (Exception?)_penultimateValue;
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
                    return ConstantBindingMemberInfo.UnsetInstance;
                return _lastMember;
            }
        }

        public IBindingMemberInfo[] Members
        {
            get
            {
                if (_lastMember == null)
                    return ConstantBindingMemberInfo.UnsetInstanceArray;
                if (_members == null)
                    return new[] { _lastMember };
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

        #endregion
    }
}