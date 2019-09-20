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
    public readonly ref struct BindingPathLastMember
    {
        #region Fields

        private readonly IBindingMemberInfo? _lastMember;
        private readonly IBindingPath _bindingPath;
        private readonly object? _penultimateValue;

        #endregion

        #region Constructors

        public BindingPathLastMember(IBindingPath bindingPath)
        {
            Should.NotBeNull(bindingPath, nameof(bindingPath));
            _bindingPath = bindingPath;
            _penultimateValue = null;
            _lastMember = null;
        }

        public BindingPathLastMember(IBindingPath bindingPath, object? penultimateValue, IBindingMemberInfo lastMember)
        {
            Should.NotBeNull(bindingPath, nameof(bindingPath));
            Should.NotBeNull(lastMember, nameof(lastMember));
            _penultimateValue = penultimateValue;
            _lastMember = lastMember;
            _bindingPath = bindingPath;
        }

        public BindingPathLastMember(IBindingPath bindingPath, Exception exception)
        {
            Should.NotBeNull(bindingPath, nameof(bindingPath));
            Should.NotBeNull(exception, nameof(exception));
            _bindingPath = bindingPath;
            _penultimateValue = exception;
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

        #endregion

        #region Methods

        public void ThrowIfError()
        {
            if (Error != null)
                throw Error!;
        }


        public object? GetLastMemberValue(IReadOnlyMetadataContext? metadata = null)
        {
            return ((IBindingPropertyInfo)LastMember).GetValue(PenultimateValue, metadata);
        }

        public void SetLastMemberValue(object? value, IReadOnlyMetadataContext? metadata = null)
        {
            ((IBindingPropertyInfo)LastMember).SetValue(PenultimateValue, value, metadata);
        }

        #endregion
    }
}