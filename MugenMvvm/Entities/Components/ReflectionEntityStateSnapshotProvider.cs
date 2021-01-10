﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
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
    public sealed class ReflectionEntityStateSnapshotProvider : IEntityStateSnapshotProviderComponent, IHasPriority, IEqualityComparer<object?>
    {
        #region Fields

        private readonly Dictionary<Type, EntityMemberAccessor[]> _cache;
        private readonly IReflectionManager? _reflectionManager;

        private Func<PropertyInfo, bool>? _memberFilter;
        private BindingFlags _memberFlags = BindingFlags.Public | BindingFlags.Instance;

        #endregion

        #region Constructors

        public ReflectionEntityStateSnapshotProvider(IReflectionManager? reflectionManager = null)
        {
            _reflectionManager = reflectionManager;
            _cache = new Dictionary<Type, EntityMemberAccessor[]>(7, InternalEqualityComparer.Type);
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

        public IEntityStateSnapshot? TryGetSnapshot(IEntityManager entityManager, object entity, IReadOnlyMetadataContext? metadata)
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
            return new EntityStateSnapshot(entity, value, this);
        }

        bool IEqualityComparer<object?>.Equals(object? x, object? y) => Equals(GetUnderlyingValue(x!), GetUnderlyingValue(y!));

        int IEqualityComparer<object?>.GetHashCode(object? key) => GetUnderlyingValue(key!).GetHashCode();

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
                    var getter = propertyInfo.GetMemberGetter<Func<object, object?>>(_reflectionManager);
                    var setter = propertyInfo.GetMemberSetter<Action<object, object?>>(_reflectionManager);
                    list.Add(new EntityMemberAccessor(propertyInfo, getter, setter));
                }
            }

            return list.List?.ToArray() ?? Default.Array<EntityMemberAccessor>();
        }

        private static object GetUnderlyingValue(object key)
        {
            if (key is MemberInfo m)
                return m.Name;
            return key;
        }

        #endregion

        #region Nested types

        private sealed class EntityStateSnapshot : Dictionary<object, MemberState>, IEntityStateSnapshot
        {
            #region Constructors

            public EntityStateSnapshot(object entity, IReadOnlyList<EntityMemberAccessor> accessors, IEqualityComparer<object> comparer)
                : base(accessors.Count, comparer)
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
                Should.BeOfType(entity, EntityType, nameof(entity));
                if (member != null)
                    return TryGetValue(member, out var value) && !Equals(value.GetValue(entity), value.Value);

                foreach (var pair in this)
                {
                    if (!Equals(pair.Value.GetValue(entity), pair.Value.Value))
                        return true;
                }

                return false;
            }

            public void Restore(object entity, IReadOnlyMetadataContext? metadata = null)
            {
                Should.BeOfType(entity, EntityType, nameof(entity));
                foreach (var pair in this)
                    pair.Value.SetValue(entity, pair.Value.Value);
            }

            public ItemOrIReadOnlyList<EntityStateValue> Dump(object entity, IReadOnlyMetadataContext? metadata = null)
            {
                Should.BeOfType(entity, EntityType, nameof(entity));
                if (Count == 0)
                    return default;
                if (Count == 1)
                {
                    var pair = this.FirstOrDefault();
                    return new EntityStateValue(pair.Key, pair.Value.Value, pair.Value.GetValue(entity));
                }

                var values = new EntityStateValue[Count];
                var index = 0;
                foreach (var pair in this)
                    values[index++] = new EntityStateValue(pair.Key, pair.Value.Value, pair.Value.GetValue(entity));
                return values;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
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

        [StructLayout(LayoutKind.Auto)]
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