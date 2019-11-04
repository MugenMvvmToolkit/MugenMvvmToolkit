using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class ReflectionMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly IObserverProvider? _bindingObserverProvider;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly CacheDictionary _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionMemberProviderComponent(IGlobalValueConverter? globalValueConverter = null,
            IObserverProvider? bindingObserverProvider = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _globalValueConverter = globalValueConverter;
            _bindingObserverProvider = bindingObserverProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _cache = new CacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IBindingMemberInfo> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var cacheKey = new CacheKey(type, name);
            if (!_cache.TryGetValue(cacheKey, out var list))
            {
                list = GetMembers(type, name, metadata);
                _cache[cacheKey] = list;
            }

            return list;
        }

        #endregion

        #region Methods

        private List<IBindingMemberInfo> GetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var indexerArgs = MugenBindingExtensions.GetIndexerArgsRaw(name);
            var types = MugenBindingExtensions.SelfAndBaseTypes(type);
            var hasProperty = false;
            var hasEvent = false;
            var hasField = false;
            var result = new List<IBindingMemberInfo>();
            foreach (var t in types)
            {
                if (!hasProperty)
                {
                    if (indexerArgs == null)
                    {
                        var property = t.GetProperty(name, BindingFlagsEx.All);
                        if (property != null)
                        {
                            result.Add(new BindingPropertyInfo(name, property, type, _bindingObserverProvider, _reflectionDelegateProvider));
                            hasProperty = true;
                        }
                    }
                    else
                    {
                        var candidate = SelectMember(t.GetProperties(BindingFlagsEx.All), name, indexerArgs, metadata, (info, _) => info.GetIndexParameters(), out var indexerValues);
                        if (candidate != null)
                        {
                            result.Add(new IndexerBindingPropertyInfo(name, candidate, indexerValues!, type, _bindingObserverProvider, _reflectionDelegateProvider));
                            hasProperty = true;
                        }
                        else if (t.IsArray && t.GetArrayRank() == indexerArgs.Length)
                        {
                            result.Add(new ArrayBindingMemberInfo(name, t, _globalValueConverter.ConvertValues<int>(indexerArgs, metadata)));
                            hasProperty = true;
                        }
                    }
                }

                if (!hasEvent)
                {
                    var eventInfo = t.GetEvent(name, BindingFlagsEx.All);
                    if (eventInfo != null)
                    {
                        var memberObserver = _bindingObserverProvider.ServiceIfNull().TryGetMemberObserver(type, eventInfo, metadata);
                        if (!memberObserver.IsEmpty)
                        {
                            result.Add(new BindingEventInfo(name, eventInfo, memberObserver));
                            hasEvent = true;
                        }
                    }
                }

                if (!hasField)
                {
                    var field = t.GetField(name, BindingFlagsEx.All);
                    if (field != null)
                    {
                        result.Add(new BindingFieldInfo(name, field, type, _bindingObserverProvider, _reflectionDelegateProvider));
                        hasField = true;
                    }
                }

                if (hasEvent && hasField && hasProperty)
                    break;
            }

            var methodArgs = MugenBindingExtensions.GetMethodArgsRaw(name, out var methodName);
            if (methodArgs == null)
            {
                foreach (var methodInfo in type.GetMethods(BindingFlagsEx.All))
                {
                    if (methodInfo.Name.Equals(name))
                        result.Add(new BindingMethodInfo(name, methodInfo, type, _bindingObserverProvider, _reflectionDelegateProvider));
                }
            }
            else
            {
                var method = SelectMember(type.GetMethods(BindingFlagsEx.All), methodName, methodArgs, metadata, (info, n) => info.Name == n ? info.GetParameters() : null, out var args);
                if (method != null)
                    result.Add(new MethodBindingPropertyInfo(name, method, args!, type, _bindingObserverProvider, _reflectionDelegateProvider));
            }

            return result;
        }

        private T? SelectMember<T, TState>(T[] members, TState state, string[] args, IReadOnlyMetadataContext? metadata, Func<T, TState, ParameterInfo[]?> getParameters, out object?[]? indexerValues) where T : MemberInfo
        {
            T? candidate = null;
            var valueTypeCount = -1;
            ParameterInfo[]? indexParameters = null;
            foreach (var member in members)
            {
                indexParameters = getParameters(member, state);
                if (indexParameters == null || indexParameters.Length != args.Length)
                    continue;

                var count = 0;
                for (var i = 0; i < indexParameters.Length; i++)
                {
                    var arg = args[i];
                    var paramType = indexParameters[i].ParameterType;
                    if (arg.StartsWith("\"", StringComparison.Ordinal) && arg.EndsWith("\"", StringComparison.Ordinal))
                    {
                        if (paramType != typeof(string))
                        {
                            count = -1;
                            break;
                        }
                    }
                    else
                    {
                        if (!_globalValueConverter.TryConvert(arg, paramType, null, metadata, out _))
                        {
                            count = -1;
                            break;
                        }

                        if (paramType.IsValueType)
                            count++;
                    }
                }

                if (valueTypeCount < count)
                {
                    candidate = member;
                    valueTypeCount = count;
                }
            }

            indexerValues = candidate == null ? null : _globalValueConverter.ConvertValues(args, indexParameters, null, metadata);
            return candidate;
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
        {
            #region Fields

            public readonly string Name;
            public readonly Type Type;

            #endregion

            #region Constructors

            public CacheKey(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            #endregion
        }

        private sealed class CacheDictionary : LightDictionary<CacheKey, IReadOnlyList<IBindingMemberInfo>>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.Type == y.Type && string.Equals(x.Name, y.Name);
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    return key.Type.GetHashCode() * 397 ^ key.Name.GetHashCode();
                }
            }

            #endregion
        }

        #endregion
    }
}