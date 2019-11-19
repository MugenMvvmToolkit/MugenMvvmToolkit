using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Internal;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class ExtensionMethodMemberProviderComponent : AttachableComponentBase<IMemberProvider>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObserverProvider? _bindingObserverProvider;
        private readonly TypeStringLightDictionary<IReadOnlyList<IMemberInfo>> _cache;
        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly Type[] _singleTypeBuffer;
        private readonly HashSet<Type> _types;

        #endregion

        #region Constructors

        public ExtensionMethodMemberProviderComponent(IGlobalValueConverter? globalValueConverter = null,
            IObserverProvider? bindingObserverProvider = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _globalValueConverter = globalValueConverter;
            _bindingObserverProvider = bindingObserverProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _singleTypeBuffer = new Type[1];
            _cache = new TypeStringLightDictionary<IReadOnlyList<IMemberInfo>>(59);
            _types = new HashSet<Type>
            {
                typeof(Enumerable)
            };
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberPriority.Extension;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IMemberInfo> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var cacheKey = new TypeStringKey(type, name);
            if (!_cache.TryGetValue(cacheKey, out var list))
            {
                list = GetMembers(type, name, metadata);
                _cache[cacheKey] = list;
            }

            return list;
        }

        #endregion

        #region Methods

        public void Add(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (_types.Add(type))
            {
                _cache.Clear();
                (Owner as IHasCache)?.Invalidate();
            }
        }

        public void Remove(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (_types.Remove(type))
            {
                _cache.Clear();
                (Owner as IHasCache)?.Invalidate();
            }
        }

        private IReadOnlyList<IMemberInfo> GetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            List<IMemberInfo>? members = null;
            var methodArgs = MugenBindingExtensions.GetMethodArgsRaw(name, out var methodName);
            foreach (var exType in _types)
            {
                var methods = exType.GetMethods(BindingFlagsEx.All);
                for (var i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    if (method.Name != methodName || !method.IsDefined(typeof(ExtensionAttribute), false))
                        continue;

                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                        continue;

                    var isGenericMethod = method.IsGenericMethodDefinition;
                    if (!parameters[0].ParameterType.IsAssignableFrom(type) && !isGenericMethod)
                        continue;

                    if (methodArgs == null)
                    {
                        Type[]? genericArgs;
                        if (isGenericMethod)
                        {
                            method = TryMakeGenericMethod(method, type, out genericArgs)!;
                            if (method == null)
                                continue;
                            parameters = method.GetParameters();
                            if (!parameters[0].ParameterType.IsAssignableFrom(type))
                                continue;
                        }
                        else
                            genericArgs = null;

                        if (members == null)
                            members = new List<IMemberInfo>();
                        members.Add(new MethodMemberInfo(name, method, true, type, _bindingObserverProvider, _reflectionDelegateProvider, parameters, genericArgs));
                        continue;
                    }

                    if (parameters.Length - 1 != methodArgs.Length)
                        continue;//todo default value

                    try
                    {
                        var values = _globalValueConverter.ConvertValues(methodArgs, parameters, null, metadata, 1);
                        if (members == null)
                            members = new List<IMemberInfo>();
                        if (isGenericMethod)
                        {
                            var types = MugenBindingExtensions.TryInferGenericParameters(method.GetGenericArguments(), method.GetParameters(),
                                info => info.ParameterType, new KeyValuePair<Type, object?[]>(type, values), (data, i) =>
                                {
                                    if (i == 0)
                                        return data.Key;
                                    return data.Value[i - 1]?.GetType();
                                }, values.Length + 1, out var hasUnresolved);

                            if (types == null || hasUnresolved)
                                continue;
                            method = method.MakeGenericMethod(types);
                            parameters = method.GetParameters();
                            if (!parameters[0].ParameterType.IsAssignableFrom(type))
                                continue;
                        }

                        members.Add(new MethodMemberAccessorInfo(name, method, values, type, _bindingObserverProvider, _reflectionDelegateProvider));
                    }
                    catch
                    {
                        ;
                    }
                }
            }

            return (IReadOnlyList<IMemberInfo>?)members ?? Default.EmptyArray<IMemberInfo>();
        }

        private MethodInfo? TryMakeGenericMethod(MethodInfo method, Type type, out Type[]? genericArguments)
        {
            try
            {
                _singleTypeBuffer[0] = type;
                genericArguments = MugenBindingExtensions.TryInferGenericParameters(method.GetGenericArguments(),
                    method.GetParameters(), info => info.ParameterType, _singleTypeBuffer, (data, i) => data[i], _singleTypeBuffer.Length, out _);
                if (genericArguments == null)
                    return null;
                return method.MakeGenericMethod(genericArguments);
            }
            catch
            {
                genericArguments = null;
                return null;
            }
        }

        #endregion
    }
}