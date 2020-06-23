using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Entities.Components
{
    public sealed class ReflectionEntityStateSnapshotProvider : IEntityStateSnapshotProviderComponent, IHasPriority
    {
        #region Fields

        private readonly MemberInfoLightDictionary<Type, EntityMemberAccessor[]> _cache;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;

        private Func<PropertyInfo, bool>? _memberFilter;
        private BindingFlags _memberFlags = BindingFlags.Public | BindingFlags.Instance;

        #endregion

        #region Constructors

        public ReflectionEntityStateSnapshotProvider(IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _cache = new MemberInfoLightDictionary<Type, EntityMemberAccessor[]>(7);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = EntityComponentPriority.SnapshotProvider;

        public BindingFlags MemberFlags
        {
            get => _memberFlags;
            set
            {
                if (value == _memberFlags)
                    return;
                _memberFlags = value;
                ClearCache();
            }
        }

        public Func<PropertyInfo, bool>? MemberFilter
        {
            get => _memberFilter;
            set
            {
                if (value == _memberFilter)
                    return;
                _memberFilter = value;
                ClearCache();
            }
        }

        #endregion

        #region Implementation of interfaces

        public IEntityStateSnapshot? TryGetSnapshot<TState>(object entity, in TState state, IReadOnlyMetadataContext? metadata)
        {
            var type = entity.GetType();
            EntityMemberAccessor[]? value;
            lock (_cache)
            {
                if (!_cache.TryGetValue(type, out value))
                {
                    value = GetAccessors(type);
                    _cache[type] = value;
                }
            }

            if (value.Length == 0)
                return null;
            return EntityStateSnapshot.Get(entity, value);
        }

        #endregion

        #region Methods

        private void ClearCache()
        {
            lock (_cache)
            {
                _cache.Clear();
            }
        }

        private EntityMemberAccessor[] GetAccessors(Type type)
        {
            var properties = type.GetProperties(MemberFlags);
            LazyList<EntityMemberAccessor> list = default;
            for (var index = 0; index < properties.Length; index++)
            {
                var propertyInfo = properties[index];
                if (propertyInfo.CanRead && propertyInfo.CanWrite && propertyInfo.GetIndexParameters().Length == 0 && (MemberFilter == null || MemberFilter(propertyInfo)))
                {
                    var getter = propertyInfo.GetMemberGetter<Func<object, object?>>(_reflectionDelegateProvider);
                    var setter = propertyInfo.GetMemberSetter<Action<object, object?>>(_reflectionDelegateProvider);
                    list.Add(new EntityMemberAccessor(propertyInfo, getter, setter));
                }
            }

            return list.List?.ToArray() ?? Default.Array<EntityMemberAccessor>();
        }

        #endregion

        #region Nested types

        private sealed class EntityStateSnapshot : LightDictionary<object, MemberState>, IEntityStateSnapshot
        {
            #region Constructors

            private EntityStateSnapshot(object entity, IReadOnlyList<EntityMemberAccessor> accessors) : base(accessors.Count)
            {
                Should.NotBeNull(entity, nameof(entity));
                Should.NotBeNull(accessors, nameof(accessors));
                EntityType = entity.GetType();
                for (var i = 0; i < accessors.Count; i++)
                {
                    var accessor = accessors[i];
                    this[accessor.Member] = new MemberState(accessor.GetValue(entity), accessor.GetValue, accessor.SetValue);
                }
            }

            #endregion

            #region Properties

            public Type EntityType { get; }

            #endregion

            #region Implementation of interfaces

            public bool HasChanges(object entity, object? member = null, IReadOnlyMetadataContext? metadata = null)
            {
                Should.BeOfType(entity, nameof(entity), EntityType);
                if (member != null)
                    return TryGetValue(member, out var value) && !object.Equals(value.GetValue(entity), value.Value);

                foreach (var pair in this)
                {
                    if (!object.Equals(pair.Value.GetValue(entity), pair.Value.Value))
                        return true;
                }

                return false;
            }

            public void Restore(object entity, IReadOnlyMetadataContext? metadata = null)
            {
                Should.BeOfType(entity, nameof(entity), EntityType);
                foreach (var pair in this)
                    pair.Value.SetValue(entity, pair.Value.Value);
            }

            public IReadOnlyList<EntityStateValue> Dump(object entity, IReadOnlyMetadataContext? metadata = null)
            {
                Should.BeOfType(entity, nameof(entity), EntityType);
                var values = new EntityStateValue[Count];
                var index = 0;
                foreach (var pair in this)
                    values[index++] = new EntityStateValue(pair.Key, pair.Value.Value, pair.Value.GetValue(entity));
                return values;
            }

            #endregion

            #region Methods

            public static IEntityStateSnapshot Get(object entity, IReadOnlyList<EntityMemberAccessor> accessors)
            {
                return new EntityStateSnapshot(entity, accessors);
            }

            protected override bool Equals(object x, object y)
            {
                return object.Equals(GetUnderlyingValue(x), GetUnderlyingValue(y));
            }

            protected override int GetHashCode(object key)
            {
                return GetUnderlyingValue(key).GetHashCode();
            }

            private static object GetUnderlyingValue(object key)
            {
                if (key is MemberInfo m)
                    return m.Name;
                return key;
            }

            #endregion
        }

        private readonly struct MemberState
        {
            #region Fields

            public readonly Func<object, object?> GetValue;
            public readonly Action<object, object?> SetValue;
            public readonly object? Value;

            #endregion

            #region Constructors

            public MemberState(object? value, Func<object, object?> getValue, Action<object, object?> setValue)
            {
                Value = value;
                GetValue = getValue;
                SetValue = setValue;
            }

            #endregion
        }

        private readonly struct EntityMemberAccessor
        {
            #region Fields

            public readonly object Member;
            public readonly Func<object, object?> GetValue;
            public readonly Action<object, object?> SetValue;

            #endregion

            #region Constructors

            public EntityMemberAccessor(object member, Func<object, object?> getValue, Action<object, object?> setValue)
            {
                Should.NotBeNull(member, nameof(member));
                Should.NotBeNull(getValue, nameof(getValue));
                Should.NotBeNull(setValue, nameof(setValue));
                Member = member;
                GetValue = getValue;
                SetValue = setValue;
            }

            #endregion
        }

        #endregion
    }
}