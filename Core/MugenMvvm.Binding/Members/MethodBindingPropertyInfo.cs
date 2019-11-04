using System;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class MethodBindingPropertyInfo : IBindingMemberAccessorInfo
    {
        #region Fields

        private readonly object?[] _args;

        private readonly MethodInfo _methodInfo;
        private readonly IObserverProvider? _observerProvider;
        private readonly Type _reflectedType;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private Func<object?, object?[], object?> _invoker;
        private MemberObserver? _observer;

        #endregion

        #region Constructors

        public MethodBindingPropertyInfo(string name, MethodInfo methodInfo, object?[] args,
            Type reflectedType, IObserverProvider? observerProvider, IReflectionDelegateProvider? reflectionDelegateProvider)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(methodInfo, nameof(methodInfo));
            Should.NotBeNull(args, nameof(args));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _methodInfo = methodInfo;
            _reflectedType = reflectedType;
            _observerProvider = observerProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _args = args;
            _invoker = CompileInvoker;
            Name = name;
            AccessModifiers = methodInfo.GetAccessModifiers();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type Type => _methodInfo.ReturnType;

        public object? Member => _methodInfo;

        public BindingMemberType MemberType => BindingMemberType.Property;

        public BindingMemberFlags AccessModifiers { get; }

        public bool CanRead => true;

        public bool CanWrite => false;

        #endregion

        #region Implementation of interfaces

        public Unsubscriber TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.ServiceIfNull().TryGetMemberObserver(_reflectedType, new MemberObserverRequest(Name, _methodInfo, _args));
            return _observer.Value.TryObserve(target, listener, metadata);
        }

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            return _invoker(target, _args);
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
        }

        #endregion

        #region Methods

        private object? CompileInvoker(object? arg, object?[] values)
        {
            _invoker = _methodInfo.GetMethodInvoker(_reflectionDelegateProvider);
            return _invoker(arg, values);
        }

        #endregion
    }
}