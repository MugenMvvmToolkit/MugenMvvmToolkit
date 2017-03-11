#region Copyright

// ****************************************************************************
// <copyright file="MugenContainer.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit
{
    public class MugenContainer : IIocContainer
    {
        #region Nested types

        private struct BindingKey
        {
            private sealed class TypeNameEqualityComparer : IEqualityComparer<BindingKey>
            {
                #region Implementation of interfaces

                public bool Equals(BindingKey x, BindingKey y)
                {
                    return Equals(x.Type, y.Type) && string.Equals(x._name, y._name);
                }

                public int GetHashCode(BindingKey obj)
                {
                    unchecked
                    {
                        return obj.Type.GetHashCode() * 397 ^ (obj._name != null ? obj._name.GetHashCode() : 0);
                    }
                }

                #endregion
            }

            public static readonly IEqualityComparer<BindingKey> TypeNameComparer = new TypeNameEqualityComparer();

            public readonly Type Type;
            private readonly string _name;

            public BindingKey(Type type, string name)
            {
                Type = type;
                _name = name;
            }
        }

        private sealed class BindingRegistration
        {
            #region Fields

            private readonly MugenContainer _container;
            private readonly DependencyLifecycle _lifecycle;
            private IIocParameter[] _parameters;
            private List<ConstructorInfo> _cachedConstructors;
            private bool _hasValue;
            private Func<IIocContainer, IList<IIocParameter>, object> _methodBindingDelegate;
            private Type _type;
            private object _value;

            #endregion

            #region Constructors

            public BindingRegistration(object value)
            {
                _hasValue = true;
                _value = value;
            }

            public BindingRegistration(MugenContainer container, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, IIocParameter[] parameters)
            {
                _container = container;
                _methodBindingDelegate = methodBindingDelegate;
                _lifecycle = lifecycle;
                _parameters = parameters;
            }

            public BindingRegistration(MugenContainer container, Type type, DependencyLifecycle lifecycle, IIocParameter[] parameters)
            {
                _container = container;
                _type = type;
                _lifecycle = lifecycle;
                _parameters = parameters;
            }

            #endregion

            #region Methods

            public object Resolve(IList<IIocParameter> parameters)
            {
                if (_hasValue)
                    return _value;

                parameters = MergeParameters(parameters);
                if (_methodBindingDelegate != null)
                {
                    if (_lifecycle == DependencyLifecycle.SingleInstance)
                    {
                        if (!_hasValue)
                        {
                            lock (this)
                            {
                                if (!_hasValue)
                                {
                                    _value = _methodBindingDelegate(_container, parameters);
                                    _hasValue = true;
                                    _methodBindingDelegate = null;
                                    _parameters = null;
                                }
                            }
                        }
                        return _value;
                    }
                    return _methodBindingDelegate.Invoke(_container, parameters);
                }

                if (_type != null)
                {
                    var constructor = FindConstructor(_type, parameters);
                    if (constructor == null)
                        throw new InvalidOperationException($"Constructor for type {_type} is null, can't activate this service.");

                    if (_lifecycle == DependencyLifecycle.SingleInstance)
                    {
                        if (!_hasValue)
                        {
                            lock (this)
                            {
                                if (!_hasValue)
                                {
                                    _value = constructor.InvokeEx(GetParameters(constructor, parameters));
                                    SetProperties(_value, parameters);
                                    _hasValue = true;
                                    _type = null;
                                    _parameters = null;
                                }
                            }
                        }
                        return _value;
                    }

                    var result = constructor.InvokeEx(GetParameters(constructor, parameters));
                    SetProperties(result, parameters);
                    return result;
                }

                //waiting activation
                lock (this)
                {
                }
                return _value;
            }

            private static void SetProperties(object item, IList<IIocParameter> parameters)
            {
                if (parameters.Count == 0)
                    return;
                var type = item.GetType();
                for (int i = 0; i < parameters.Count; i++)
                {
                    var iocParameter = parameters[i];
                    if (iocParameter.ParameterType != IocParameterType.Property)
                        continue;
#if NET4
                    var propertyInfo = type.GetProperty(iocParameter.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (propertyInfo.CanWrite)
                        propertyInfo.SetValueEx(item, iocParameter.Value);
#else
                    var propertyInfo = type.GetRuntimeProperty(iocParameter.Name);
                    var methodInfo = propertyInfo.SetMethod;
                    if (methodInfo != null && !methodInfo.IsStatic && methodInfo.IsPublic)
                        propertyInfo.SetValueEx(item, iocParameter.Value);
#endif
                }
            }

            private IList<IIocParameter> MergeParameters(IList<IIocParameter> parameters)
            {
                if (_parameters == null || _parameters.Length == 0)
                    return parameters ?? Empty.Array<IIocParameter>();
                if (parameters == null || parameters.Count == 0)
                    return _parameters ?? Empty.Array<IIocParameter>();
                var iocParameters = new List<IIocParameter>(parameters);
                iocParameters.AddRange(_parameters);
                return iocParameters;
            }

            private object[] GetParameters(ConstructorInfo constructor, IList<IIocParameter> parameters)
            {
                var parameterInfos = constructor.GetParameters();
                if (parameterInfos.Length == 0)
                    return Empty.Array<object>();
                var result = new object[parameterInfos.Length];
                //Find constructor arguments
                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    var find = false;
                    for (var index = 0; index < parameters.Count; index++)
                    {
                        var injectionParameter = parameters[index];
                        if (!CanResolve(injectionParameter, parameterInfo))
                            continue;
                        result[i] = injectionParameter.Value;
                        find = true;
                        break;
                    }
                    if (find)
                        continue;
                    var resolve = ResolveParameter(parameterInfo.ParameterType, parameterInfo);
                    result[i] = resolve;
                }

                return result;
            }

            private object ResolveParameter(Type type, ParameterInfo parameterInfo)
            {
                //Use default parameter value.
                if (parameterInfo.IsOptional && !_container.CanResolve(type))
                    return parameterInfo.DefaultValue;

                var hasParamsAttr = parameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
                //If have ParamArrayAttribute.
                if (hasParamsAttr)
                {
#if NET4
                    var originalType = type.GetElementType();
#else
                    var originalType = type.GetTypeInfo().GetElementType();
#endif

                    //If exist array binding.
                    if (_container.CanResolve(type))
                        return _container.Get(type);

                    //If exist binding for type.
                    if (_container.CanResolve(originalType))
                        return ConvertToArray(type, _container.GetAll(originalType));
                    return ConvertToArray(type, new object[0]);
                }
                return _container.Get(type);
            }

            private static object ConvertToArray(Type arrayType, object obj)
            {
                if (obj == null)
                    return null;
                var objects = obj as Array;
                if (objects == null)
                    objects = new[] { obj };
                var array = (Array)Activator.CreateInstance(arrayType, objects.Length);
                for (var i = 0; i < objects.Length; i++)
                    array.SetValue(objects.GetValue(i), i);
                return array;
            }

            private ConstructorInfo FindConstructor(Type service, IList<IIocParameter> parameters)
            {
                ConstructorInfo result = null;
                var bestCount = -1;
                var currentCountParameter = 0;
                List<ConstructorInfo> constructorInfos;
                lock (this)
                {
                    constructorInfos = GetConstuctors(service);
                }
                if (constructorInfos.Count == 1)
                    return constructorInfos[0];
                foreach (var constructorInfo in constructorInfos)
                {
                    var currentCount = 0;
                    var parameterInfos = constructorInfo.GetParameters();
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        var parameterInfo = parameterInfos[i];
                        for (var index = 0; index < parameters.Count; index++)
                        {
                            var parameter = parameters[index];
                            if (CanResolve(parameter, parameterInfo))
                            {
                                currentCount++;
                                break;
                            }
                        }
                    }
                    if (bestCount > currentCount) continue;
                    if (bestCount == currentCount && parameterInfos.Length > currentCountParameter) continue;
                    currentCountParameter = parameterInfos.Length;
                    result = constructorInfo;
                    bestCount = currentCount;
                }
                return result;
            }

            private static bool CanResolve(IIocParameter parameter, ParameterInfo parameterInfo)
            {
                return parameter.ParameterType == IocParameterType.Constructor && parameter.Name == parameterInfo.Name;
            }

            private List<ConstructorInfo> GetConstuctors(Type service)
            {
                if (_cachedConstructors == null)
                {
#if NET4
                    _cachedConstructors = new List<ConstructorInfo>(service.GetConstructors());
#else
                    _cachedConstructors = new List<ConstructorInfo>(service.GetTypeInfo().DeclaredConstructors);
#endif
                    for (var index = 0; index < _cachedConstructors.Count; index++)
                    {
                        var constructorInfo = _cachedConstructors[index];
                        if (constructorInfo.IsStatic || !constructorInfo.IsPublic)
                        {
                            _cachedConstructors.Remove(constructorInfo);
                            index--;
                        }
                    }
                }
                return _cachedConstructors;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Dictionary<BindingKey, List<BindingRegistration>> _bindingRegistrations;
        private readonly MugenContainer _parent;
        private readonly Dictionary<Type, BindingRegistration> _selfActivatedRegistrations;

        private static int _idCounter;

        #endregion

        #region Constructors

        public MugenContainer()
        {
            _bindingRegistrations = new Dictionary<BindingKey, List<BindingRegistration>>(BindingKey.TypeNameComparer);
            _selfActivatedRegistrations = new Dictionary<Type, BindingRegistration>();
            Id = Interlocked.Increment(ref _idCounter);
        }

        private MugenContainer(MugenContainer parent)
            : this()
        {
            _parent = parent;
        }

        #endregion

        #region Properties

        public bool IsDisposed { get; private set; }

        public int Id { get; }

        public IIocContainer Parent => _parent;

        public object Container => this;

        #endregion

        #region Methods

        private static bool IsSelfBindableType(Type service)
        {
#if NET4
            return !service.IsInterface
                   && !service.IsValueType
                   && service != typeof(string)
                   && !service.IsAbstract
                   && !service.ContainsGenericParameters;
#else
            var typeInfo = service.GetTypeInfo();
            return !typeInfo.IsInterface
                   && !typeInfo.IsValueType
                   && service != typeof(string)
                   && !typeInfo.IsAbstract
                   && !typeInfo.ContainsGenericParameters;
#endif            
        }

        private IList<object> GetAll(Type service, string name = null, params IIocParameter[] parameters)
        {
            Should.NotBeNull(service, nameof(service));
            var key = new BindingKey(service, name);
            List<BindingRegistration> registrations;
            lock (_bindingRegistrations)
                _bindingRegistrations.TryGetValue(key, out registrations);
            if (registrations != null && registrations.Count > 0)
            {
                var result = new object[registrations.Count];
                for (int i = 0; i < registrations.Count; i++)
                    result[i] = registrations[i].Resolve(parameters);
                return result;
            }
            if (_parent != null)
                return _parent.GetAll(service, name, parameters);

            object value;
            if (TryResolve(service, parameters, out value))
                return new[] { value };
            return Empty.Array<object>();
        }

        private object Resolve(Type service, IIocParameter[] parameters)
        {
            object value;
            if (TryResolve(service, parameters, out value))
                return value;
            throw new InvalidOperationException($"For service {service}, binding not found.");
        }

        private bool TryResolve(Type service, IIocParameter[] parameters, out object value)
        {
            if (service.IsArray)
            {
                Type elementType = service.GetElementType();
                value = ConvertToArray(service, elementType);
                return true;
            }

#if NET4
            if (!service.IsGenericType)
#else
            if (!service.GetTypeInfo().IsGenericType)
#endif
                return TryResolveSelfBindable(service, parameters, out value);


            Type definition = service.GetGenericTypeDefinition();
            ConstructorInfo constructor = null;
            var originalType = service.GetGenericArguments()[0];
#if NET4
            if (definition.IsInterface)
#else
            if (definition.GetTypeInfo().IsInterface)
#endif
            {
                if (definition == typeof(ICollection<>) || definition == typeof(IEnumerable<>) || definition == typeof(IList<>)
#if !NET4
      || definition == typeof(IReadOnlyCollection<>) || definition == typeof(IReadOnlyList<>)
#endif
                    )
                    constructor = typeof(List<>).MakeGenericType(originalType).GetConstructor(Empty.Array<Type>());
            }
            else
            {
                if (typeof(ICollection<>).MakeGenericType(originalType).IsAssignableFrom(service))
                    constructor = service.GetConstructor(Empty.Array<Type>());
            }

            if (constructor == null)
                return TryResolveSelfBindable(service, parameters, out value);
#if NET4
            MethodInfo methodInfo = constructor.DeclaringType?.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, new[] { originalType }, null);
#else
            MethodInfo methodInfo = constructor.DeclaringType?.GetRuntimeMethod("Add", new[] { originalType });
#endif
            if (methodInfo == null || methodInfo.IsStatic)
                return TryResolveSelfBindable(service, parameters, out value);

            IList<object> objects = GetAll(originalType);
            value = constructor.InvokeEx();
            var args = new object[1];
            foreach (object o in objects)
            {
                args[0] = o;
                methodInfo.InvokeEx(value, args);
            }
            return true;
        }

        private bool TryResolveSelfBindable(Type service, IIocParameter[] parameters, out object value)
        {
            BindingRegistration registration;
            lock (_selfActivatedRegistrations)
            {
                if (!_selfActivatedRegistrations.TryGetValue(service, out registration))
                {
                    if (IsSelfBindableType(service))
                        registration = new BindingRegistration(this, service, DependencyLifecycle.TransientInstance, Empty.Array<IIocParameter>());
                    _selfActivatedRegistrations[service] = registration;
                }
            }
            if (registration == null)
            {
                value = null;
                return false;
            }
            value = registration.Resolve(parameters);
            return true;
        }

        private object ConvertToArray(Type arrayType, Type elementType)
        {
            IList<object> objects = GetAll(elementType);
            if (objects.Count == 1 && arrayType.IsInstanceOfType(objects[0]))
                return Array.CreateInstance(elementType, 0);

            Array array = Array.CreateInstance(elementType, objects.Count);
            for (int i = 0; i < objects.Count; i++)
                array.SetValue(objects[i], i);
            return array;
        }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            lock (_bindingRegistrations)
            {
                _bindingRegistrations.Clear();
            }
            lock (_selfActivatedRegistrations)
            {
                _selfActivatedRegistrations.Clear();
            }
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        object IServiceProvider.GetService(Type serviceType)
        {
            return Get(serviceType);
        }

        public IIocContainer CreateChild()
        {
            this.NotBeDisposed();
            return new MugenContainer(this);
        }

        public bool CanResolve(Type service, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                if (_bindingRegistrations.ContainsKey(key))
                    return true;
            }
            if (_parent == null)
                return IsSelfBindableType(service);
            return _parent.CanResolve(service, name);
        }

        public void Unbind(Type service)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            lock (_bindingRegistrations)
            {
                var keys = _bindingRegistrations.Keys.Where(key => key.Type == service).ToArray();
                foreach (var bindingKey in keys)
                    _bindingRegistrations.Remove(bindingKey);
            }
        }

        public void Bind(Type service, Type typeTo, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(typeTo, nameof(typeTo));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                List<BindingRegistration> list;
                if (!_bindingRegistrations.TryGetValue(key, out list))
                {
                    list = new List<BindingRegistration>();
                    _bindingRegistrations[key] = list;
                }
                list.Add(new BindingRegistration(this, typeTo, lifecycle, parameters));
            }
        }

        public void BindToMethod(Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(methodBindingDelegate, nameof(methodBindingDelegate));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                List<BindingRegistration> list;
                if (!_bindingRegistrations.TryGetValue(key, out list))
                {
                    list = new List<BindingRegistration>();
                    _bindingRegistrations[key] = list;
                }
                list.Add(new BindingRegistration(this, methodBindingDelegate, lifecycle, parameters));
            }
        }

        public void BindToConstant(Type service, object instance, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                List<BindingRegistration> list;
                if (!_bindingRegistrations.TryGetValue(key, out list))
                {
                    list = new List<BindingRegistration>();
                    _bindingRegistrations[key] = list;
                }
                list.Add(new BindingRegistration(instance));
            }
        }

        IEnumerable<object> IIocContainer.GetAll(Type service, string name, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            return GetAll(service, name, parameters);
        }

        public object Get(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            var key = new BindingKey(service, name);
            BindingRegistration registration = null;
            lock (_bindingRegistrations)
            {
                List<BindingRegistration> list;
                if (_bindingRegistrations.TryGetValue(key, out list))
                {
                    if (list.Count > 1)
                        throw new InvalidOperationException($"For type {service}, you have more that once binding");
                    if (list.Count == 1)
                        registration = list[0];
                }
            }
            if (registration != null)
                return registration.Resolve(parameters);
            if (_parent != null)
                return _parent.Get(service, name, parameters);
            return Resolve(service, parameters);
        }

        #endregion
    }
}
