using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Internal;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class ExtensionMethodMemberProviderComponent : AttachableComponentBase<IMemberManager>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObserverProvider? _bindingObserverProvider;
        private readonly TypeStringLightDictionary<IReadOnlyList<IMemberInfo>?> _cache;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly Type[] _singleTypeBuffer;
        private readonly HashSet<Type> _types;

        #endregion

        #region Constructors

        public ExtensionMethodMemberProviderComponent(IObserverProvider? bindingObserverProvider = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _bindingObserverProvider = bindingObserverProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _singleTypeBuffer = new Type[1];
            _cache = new TypeStringLightDictionary<IReadOnlyList<IMemberInfo>?>(59);
            _types = new HashSet<Type>
            {
                typeof(Enumerable)
            };
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Extension;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var cacheKey = new TypeStringKey(type, name);
            if (!_cache.TryGetValue(cacheKey, out var list))
            {
                list = GetMembers(type, name);
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
                Owner.TryInvalidateCache();
            }
        }

        public void Remove(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (_types.Remove(type))
            {
                _cache.Clear();
                Owner.TryInvalidateCache();
            }
        }

        private IReadOnlyList<IMemberInfo>? GetMembers(Type type, string name)
        {
            LazyList<IMemberInfo> members = default;
            foreach (var exType in _types)
            {
                var methods = exType.GetMethods(BindingFlagsEx.All);
                for (var i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    if (method.Name != name || !method.IsDefined(typeof(ExtensionAttribute), false))
                        continue;

                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                        continue;

                    if (parameters[0].ParameterType.IsAssignableFrom(type))
                    {
                        members.Add(new MethodMemberInfo(name, method, true, type, _bindingObserverProvider, _reflectionDelegateProvider, parameters, null));
                        continue;
                    }

                    if (!method.IsGenericMethodDefinition)
                        continue;

                    method = TryMakeGenericMethod(method, type, out var genericArgs)!;
                    if (method == null)
                        continue;

                    parameters = method.GetParameters();
                    if (parameters[0].ParameterType.IsAssignableFrom(type))
                        members.Add(new MethodMemberInfo(name, method, true, type, _bindingObserverProvider, _reflectionDelegateProvider, parameters, genericArgs));
                }
            }

            return members.List;
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