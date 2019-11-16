using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Internal;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class ReflectionMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObserverProvider? _bindingObserverProvider;
        private readonly TypeStringLightDictionary<IReadOnlyList<IMemberInfo>> _cache;
        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionMemberProviderComponent(IGlobalValueConverter? globalValueConverter = null,
            IObserverProvider? bindingObserverProvider = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _globalValueConverter = globalValueConverter;
            _bindingObserverProvider = bindingObserverProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _cache = new TypeStringLightDictionary<IReadOnlyList<IMemberInfo>>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberPriority.Default;

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

        private List<IMemberInfo> GetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var hasProperty = false;
            var hasEvent = false;
            var hasField = false;
            var indexerArgs = MugenBindingExtensions.GetIndexerArgsRaw(name);
            var result = new List<IMemberInfo>();
            foreach (var t in MugenBindingExtensions.SelfAndBaseTypes(type))
            {
                if (!hasProperty)
                    hasProperty = AddProperties(type, t, name, indexerArgs, result, metadata);

                if (!hasEvent)
                    hasEvent = AddEvents(type, t, name, result, metadata);

                if (!hasField)
                    hasField = AddFields(type, t, name, result);

                if (hasEvent && hasField && hasProperty)
                    break;
            }

            var methodArgs = MugenBindingExtensions.GetMethodArgsRaw(name, out var methodName);
            var methods = type.GetMethods(BindingFlagsEx.All);
            for (var index = 0; index < methods.Length; index++)
            {
                var methodInfo = methods[index];
                if (methodInfo.Name != name)
                    continue;
                if (methodArgs == null)
                {
                    result.Add(new MethodMemberInfo(name, methodInfo, false, type, _bindingObserverProvider, _reflectionDelegateProvider));
                    continue;
                }

                var parameters = methodInfo.GetParameters();
                if (parameters.Length != methodArgs.Length)
                    continue;

                try
                {
                    var values = _globalValueConverter.ConvertValues(methodArgs, parameters, null, metadata);
                    result.Add(new MethodMemberAccessorInfo(name, methodInfo, values, type, _bindingObserverProvider, _reflectionDelegateProvider));
                }
                catch
                {
                    ;
                }
            }

            return result;
        }

        private bool AddEvents(Type requestedType, Type t, string name, List<IMemberInfo> result, IReadOnlyMetadataContext? metadata)
        {
            var eventInfo = t.GetEvent(name, BindingFlagsEx.All);
            if (eventInfo == null)
                return false;

            var memberObserver = _bindingObserverProvider.ServiceIfNull().TryGetMemberObserver(requestedType, eventInfo, metadata);
            if (memberObserver.IsEmpty)
                return false;

            result.Add(new EventMemberInfo(name, eventInfo, memberObserver));
            return true;
        }

        private bool AddFields(Type requestedType, Type t, string name, List<IMemberInfo> result)
        {
            var field = t.GetField(name, BindingFlagsEx.All);
            if (field == null)
                return false;

            result.Add(new FieldMemberAccessorInfo(name, field, requestedType, _bindingObserverProvider, _reflectionDelegateProvider));
            return true;
        }

        private bool AddProperties(Type requestedType, Type t, string name, string[]? indexerArgs, List<IMemberInfo> result, IReadOnlyMetadataContext? metadata)
        {
            if (indexerArgs == null)
            {
                var property = t.GetProperty(name, BindingFlagsEx.All);
                if (property == null)
                    return false;

                result.Add(new PropertyMemberAccessorInfo(name, property, requestedType, _bindingObserverProvider, _reflectionDelegateProvider));
                return true;
            }

            if (t.IsArray && t.GetArrayRank() == indexerArgs.Length)
            {
                try
                {
                    result.Add(new ArrayMemberAccessorInfo(name, t, _globalValueConverter.ConvertValues<int>(indexerArgs, metadata)));
                    return true;
                }
                catch
                {
                    ;
                }

                return false;
            }

            var hasProperty = false;
            var properties = t.GetProperties(BindingFlagsEx.All);
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var parameters = property.GetIndexParameters();
                if (parameters.Length != indexerArgs.Length)
                    continue;

                try
                {
                    var values = _globalValueConverter.ConvertValues(indexerArgs, parameters, null, metadata);
                    result.Add(new IndexerMemberAccessorInfo(name, property, values, requestedType, _bindingObserverProvider, _reflectionDelegateProvider));
                    hasProperty = true;
                }
                catch
                {
                    ;
                }
            }

            return hasProperty;
        }

        #endregion
    }
}