using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Internal;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class ReflectionMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObserverProvider? _bindingObserverProvider;
        private readonly TypeStringLightDictionary<IReadOnlyList<IMemberInfo>?> _cache;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionMemberProviderComponent(IObserverProvider? bindingObserverProvider = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _bindingObserverProvider = bindingObserverProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _cache = new TypeStringLightDictionary<IReadOnlyList<IMemberInfo>?>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Instance;

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
            LazyList<IMemberInfo> result = default;
            var types = MugenBindingExtensions.SelfAndBaseTypes(type);
            foreach (var t in types)
            {
                if (!hasProperty)
                    hasProperty = AddProperties(type, t, name, ref result);

                if (!hasEvent)
                    hasEvent = AddEvents(type, t, name, ref result, metadata);

                if (!hasField)
                    hasField = AddFields(type, t, name, ref result);

                if (hasEvent && hasField && hasProperty)
                    break;
            }

            types.Clear();
            foreach (var t in MugenBindingExtensions.SelfAndBaseTypes(type, false, types: types))
            {
                var methods = t.GetMethods(BindingFlagsEx.All);
                for (var index = 0; index < methods.Length; index++)
                {
                    var methodInfo = methods[index];
                    if (methodInfo.Name == name)
                        result.Add(new MethodMemberInfo(name, methodInfo, false, type, _bindingObserverProvider, _reflectionDelegateProvider));
                }
            }

            return result;
        }

        private bool AddEvents(Type requestedType, Type t, string name, ref LazyList<IMemberInfo> result, IReadOnlyMetadataContext? metadata)
        {
            var eventInfo = t.GetEvent(name, BindingFlagsEx.All);
            if (eventInfo == null)
                return false;

            var memberObserver = _bindingObserverProvider.DefaultIfNull().GetMemberObserver(requestedType, eventInfo, metadata);
            if (memberObserver.IsEmpty)
                return false;

            result.Add(new EventMemberInfo(name, eventInfo, memberObserver));
            return true;
        }

        private bool AddFields(Type requestedType, Type t, string name, ref LazyList<IMemberInfo> result)
        {
            var field = t.GetField(name, BindingFlagsEx.All);
            if (field == null)
                return false;

            result.Add(new FieldMemberAccessorInfo(name, field, requestedType, _bindingObserverProvider, _reflectionDelegateProvider));
            return true;
        }

        private bool AddProperties(Type requestedType, Type t, string name, ref LazyList<IMemberInfo> result)
        {
            var property = t.GetProperty(name, BindingFlagsEx.All);
            if (property == null)
                return false;

            result.Add(new PropertyMemberAccessorInfo(name, property, requestedType, _bindingObserverProvider, _reflectionDelegateProvider));
            return true;
        }

        #endregion
    }
}