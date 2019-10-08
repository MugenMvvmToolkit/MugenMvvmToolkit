using System;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    internal sealed class IndexerBindingPropertyInfo : IBindingPropertyInfo
    {
        #region Fields

        private readonly object?[]? _indexerValues;

        private readonly MemberObserver _observer;
        private readonly PropertyInfo _propertyInfo;

        private Func<object?, object?[], object?> _getterIndexerFunc;
        private Func<object?, object?[], object?> _setterIndexerFunc;

        #endregion

        #region Constructors

        public IndexerBindingPropertyInfo(string name, PropertyInfo propertyInfo, ParameterInfo[] indexParameters, string[] indexerValues, MemberObserver observer)
        {
            _propertyInfo = propertyInfo;
            _observer = observer;
            Name = name;
            Type = _propertyInfo.PropertyType;
            _indexerValues = BindingMugenExtensions.GetIndexerValues(indexerValues!, indexParameters);

            var getMethod = propertyInfo.GetGetMethodUnified(true);
            if (getMethod == null)
            {
                CanRead = false;
                _getterIndexerFunc = MustBeReadable;
            }
            else
            {
                CanRead = true;
                _getterIndexerFunc = CompileIndexerGetter;
            }

            var setMethod = propertyInfo.GetSetMethodUnified(true);
            if (setMethod == null)
            {
                CanWrite = false;
                _setterIndexerFunc = MustBeWritable;
            }
            else
            {
                CanWrite = true;
                _setterIndexerFunc = CompileIndexerSetter;
            }

            AccessModifiers = (getMethod ?? setMethod).GetAccessModifiers();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type Type { get; }

        public object? Member => _propertyInfo;

        public BindingMemberType MemberType => BindingMemberType.Property;

        public MemberFlags AccessModifiers { get; }

        public bool CanRead { get; }

        public bool CanWrite { get; }

        #endregion

        #region Implementation of interfaces

        public IDisposable? TryObserve(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return _observer.TryObserve(source, listener, metadata);
        }

        public object? GetValue(object? source, IReadOnlyMetadataContext? metadata = null)
        {
            return _getterIndexerFunc(source, _indexerValues!);
        }

        public void SetValue(object? source, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            var args = new object?[_indexerValues!.Length + 1];
            Array.Copy(_indexerValues, args, _indexerValues.Length);
            args[_indexerValues.Length] = value;
            _setterIndexerFunc(source, args);
        }

        #endregion

        #region Methods

        private object? MustBeWritable(object? _, object? __)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
            return null;
        }

        private object? MustBeReadable(object? _, object?[] __)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
            return null;
        }

        private object? CompileIndexerSetter(object? arg1, object?[] arg2)
        {
            _setterIndexerFunc = _propertyInfo.GetSetMethodUnified(true)!.GetMethodInvoker();
            return _setterIndexerFunc(arg1, arg2);
        }

        private object? CompileIndexerGetter(object? arg, object?[] values)
        {
            _getterIndexerFunc = _propertyInfo.GetGetMethodUnified(true)!.GetMethodInvoker();
            return _getterIndexerFunc(arg, values);
        }

        #endregion
    }
}