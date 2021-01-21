using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class ReflectionMemberProvider : IMemberProviderComponent, IEqualityComparer<ReflectionMemberProvider.CacheKey>, IHasPriority
    {
        private readonly IObservationManager? _observationManager;
        private readonly HashSet<Type> _types;
        private readonly Dictionary<CacheKey, object?> _cache;

        [Preserve(Conditional = true)]
        public ReflectionMemberProvider(IObservationManager? observationManager = null)
        {
            _observationManager = observationManager;
            _types = new HashSet<Type>(InternalEqualityComparer.Type);
            _cache = new Dictionary<CacheKey, object?>(this);
        }

        public int Priority { get; set; } = MemberComponentPriority.Instance;

        private static void AddMethodsInternal(Type requestedType, Type t, string name, ref ItemOrListEditor<IMemberInfo> result)
        {
            var isGetter = name == BindingInternalConstant.IndexerGetterName;
            var isSetter = name == BindingInternalConstant.IndexerSetterName;
            if (isGetter || isSetter)
            {
                var propertyInfos = t.GetProperties(BindingFlagsEx.All);
                for (var i = 0; i < propertyInfos.Length; i++)
                {
                    var propertyInfo = propertyInfos[i];
                    var indexParameters = propertyInfo.GetIndexParameters();
                    if (indexParameters.Length > 0)
                    {
                        var method = isGetter ? propertyInfo.GetGetMethod(true) : propertyInfo.GetSetMethod(true);
                        if (method != null)
                            result.Add(new MethodMemberInfo(name, method, false, requestedType, isGetter ? indexParameters : null, null));
                    }
                }
            }
            else
            {
                var methods = t.GetMethods(BindingFlagsEx.All);
                for (var index = 0; index < methods.Length; index++)
                {
                    var methodInfo = methods[index];
                    if (methodInfo.Name == name)
                        result.Add(new MethodMemberInfo(name, methodInfo, false, requestedType));
                }
            }
        }

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            _types.Clear();
            var hasProperty = !memberTypes.HasFlag(MemberType.Accessor);
            var hasField = hasProperty;
            var hasEvent = !memberTypes.HasFlag(MemberType.Event);
            var result = new ItemOrListEditor<IMemberInfo>();
            var types = BindingMugenExtensions.SelfAndBaseTypes(type, types: _types);
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

            if (memberTypes.HasFlag(MemberType.Method))
            {
                types.Clear();
                foreach (var t in BindingMugenExtensions.SelfAndBaseTypes(type, false, types: types))
                    AddMethods(type, t, name, ref result);
            }

            return result.ToItemOrList();
        }

        private void AddMethods(Type requestedType, Type t, string name, ref ItemOrListEditor<IMemberInfo> result)
        {
            var key = new CacheKey(CacheKey.Method, name, t);
            if (!_cache.TryGetValue(key, out var v))
            {
                ItemOrListEditor<IMemberInfo> methods = default;
                AddMethodsInternal(requestedType, t, name, ref methods);
                v = methods.GetRawValueInternal();
                _cache[key] = v;
            }

            if (v != null)
                result.AddRange(ItemOrIEnumerable.FromRawValue<IMemberInfo>(v));
        }

        private bool AddEvents(Type requestedType, Type t, string name, ref ItemOrListEditor<IMemberInfo> result, IReadOnlyMetadataContext? metadata)
        {
            var key = new CacheKey(CacheKey.Event, name, t);
            if (!_cache.TryGetValue(key, out var v))
            {
                var eventInfo = t.GetEvent(name, BindingFlagsEx.All);
                if (eventInfo != null)
                {
                    var memberObserver = _observationManager.DefaultIfNull().TryGetMemberObserver(requestedType, eventInfo, metadata);
                    if (!memberObserver.IsEmpty)
                        v = new EventMemberInfo(name, eventInfo, memberObserver);
                }

                _cache[key] = v;
            }

            if (v == null)
                return false;
            result.Add((IMemberInfo) v);
            return true;
        }

        private bool AddFields(Type requestedType, Type t, string name, ref ItemOrListEditor<IMemberInfo> result)
        {
            var key = new CacheKey(CacheKey.Field, name, t);
            if (!_cache.TryGetValue(key, out var v))
            {
                var field = t.GetField(name, BindingFlagsEx.All);
                if (field != null)
                    v = new FieldAccessorMemberInfo(name, field, requestedType);
                _cache[key] = v;
            }

            if (v == null)
                return false;
            result.Add((IMemberInfo) v);
            return true;
        }

        private bool AddProperties(Type requestedType, Type t, string name, ref ItemOrListEditor<IMemberInfo> result)
        {
            var key = new CacheKey(CacheKey.Property, name, t);
            if (!_cache.TryGetValue(key, out var v))
            {
                var property = t.GetProperty(name, BindingFlagsEx.All);
                if (property != null)
                    v = new PropertyAccessorMemberInfo(name, property, requestedType);
                _cache[key] = v;
            }

            if (v == null)
                return false;
            result.Add((IMemberInfo) v);
            return true;
        }

        bool IEqualityComparer<CacheKey>.Equals(CacheKey x, CacheKey y) => x.MemberType == y.MemberType && x.Name == y.Name && x.Type == y.Type;

        int IEqualityComparer<CacheKey>.GetHashCode(CacheKey obj) => HashCode.Combine(obj.MemberType, obj.Name, obj.Type);

        internal readonly struct CacheKey
        {
            public const int Field = 1;
            public const int Property = 2;
            public const int Method = 3;
            public const int Event = 4;

            public readonly int MemberType;
            public readonly string Name;
            public readonly Type Type;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CacheKey(int memberType, string name, Type type)
            {
                MemberType = memberType;
                Name = name;
                Type = type;
            }
        }
    }
}