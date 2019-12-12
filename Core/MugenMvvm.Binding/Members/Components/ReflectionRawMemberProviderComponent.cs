using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Internal;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class ReflectionRawMemberProviderComponent : IRawMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObserverProvider? _bindingObserverProvider;
        private readonly TypeStringLightDictionary<IReadOnlyList<IMemberInfo>?> _cache;
        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionRawMemberProviderComponent(IGlobalValueConverter? globalValueConverter = null,
            IObserverProvider? bindingObserverProvider = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _globalValueConverter = globalValueConverter;
            _bindingObserverProvider = bindingObserverProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _cache = new TypeStringLightDictionary<IReadOnlyList<IMemberInfo>?>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Reflection;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
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

        private List<IMemberInfo>? GetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var hasProperty = false;
            var hasEvent = false;
            var hasField = false;
            var indexerArgs = MugenBindingExtensions.GetIndexerArgsRaw(name);
            List<IMemberInfo>? result = null;
            foreach (var t in MugenBindingExtensions.SelfAndBaseTypes(type))
            {
                if (!hasProperty)
                    hasProperty = AddProperties(type, t, name, indexerArgs, ref result, metadata);

                if (!hasEvent)
                    hasEvent = AddEvents(type, t, name, ref result, metadata);

                if (!hasField)
                    hasField = AddFields(type, t, name, ref result);

                if (hasEvent && hasField && hasProperty)
                    break;
            }

            var methodArgs = MugenBindingExtensions.GetMethodArgsRaw(name, out var methodName);
            var methods = type.GetMethods(BindingFlagsEx.All);
            for (var index = 0; index < methods.Length; index++)
            {
                var methodInfo = methods[index];
                if (methodInfo.Name != methodName)
                    continue;

                if (result == null)
                    result = new List<IMemberInfo>();
                result.Add(new MethodMemberInfo(name, methodInfo, false, type, _bindingObserverProvider, _reflectionDelegateProvider));

                if (methodArgs == null)
                    continue;

                var parameters = methodInfo.GetParameters();
                if (parameters.Length != methodArgs.Length)
                    continue;//todo default value

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

        private bool AddEvents(Type requestedType, Type t, string name, ref List<IMemberInfo>? result, IReadOnlyMetadataContext? metadata)
        {
            var eventInfo = t.GetEvent(name, BindingFlagsEx.All);
            if (eventInfo == null)
                return false;

            var memberObserver = _bindingObserverProvider.DefaultIfNull().GetMemberObserver(requestedType, eventInfo, metadata);
            if (memberObserver.IsEmpty)
                return false;

            if (result == null)
                result = new List<IMemberInfo>();
            result.Add(new EventMemberInfo(name, eventInfo, memberObserver));
            return true;
        }

        private bool AddFields(Type requestedType, Type t, string name, ref List<IMemberInfo>? result)
        {
            var field = t.GetField(name, BindingFlagsEx.All);
            if (field == null)
                return false;

            if (result == null)
                result = new List<IMemberInfo>();
            result.Add(new FieldMemberAccessorInfo(name, field, requestedType, _bindingObserverProvider, _reflectionDelegateProvider));
            return true;
        }

        private bool AddProperties(Type requestedType, Type t, string name, string[]? indexerArgs, ref List<IMemberInfo>? result, IReadOnlyMetadataContext? metadata)
        {
            if (indexerArgs == null)
            {
                var property = t.GetProperty(name, BindingFlagsEx.All);
                if (property == null)
                    return false;

                if (result == null)
                    result = new List<IMemberInfo>();
                result.Add(new PropertyMemberAccessorInfo(name, property, requestedType, _bindingObserverProvider, _reflectionDelegateProvider));
                return true;
            }

            if (t.IsArray && t.GetArrayRank() == indexerArgs.Length)
            {
                try
                {
                    var accessorInfo = new ArrayMemberAccessorInfo(name, t, _globalValueConverter.ConvertValues<int>(indexerArgs, metadata));
                    if (result == null)
                        result = new List<IMemberInfo>();
                    result.Add(accessorInfo);
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
                    var accessorInfo = new IndexerMemberAccessorInfo(name, property, _globalValueConverter.ConvertValues(indexerArgs, parameters, null, metadata), requestedType, _bindingObserverProvider, _reflectionDelegateProvider);
                    if (result == null)
                        result = new List<IMemberInfo>();
                    result.Add(accessorInfo);
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