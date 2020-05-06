using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Internal;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class AttachedMemberProvider : AttachableComponentBase<IMemberManager>, IAttachedMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly TypeStringLightDictionary<object?> _cache;
        private readonly StringOrdinalLightDictionary<List<IMemberInfo>> _registeredMembers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedMemberProvider()
        {
            _registeredMembers = new StringOrdinalLightDictionary<List<IMemberInfo>>(59);
            _cache = new TypeStringLightDictionary<object?>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Attached;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var key = new TypeStringKey(type, name);
            if (!_cache.TryGetValue(key, out var list))
            {
                if (_registeredMembers.TryGetValue(name, out var members))
                {
                    ItemOrList<IMemberInfo, List<IMemberInfo>> result = default;
                    for (var index = 0; index < members.Count; index++)
                    {
                        var memberInfo = members[index];
                        if (type.IsAssignableFromGeneric(memberInfo.DeclaringType))
                            result.Add(memberInfo);
                    }

                    list = result.GetRawValue();
                }

                _cache[key] = list;
            }

            return ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>.FromRawValue(list);
        }

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> GetAttachedMembers(IReadOnlyMetadataContext? metadata)
        {
            ItemOrList<IMemberInfo, List<IMemberInfo>> members = default;
            foreach (var keyValuePair in _registeredMembers)
                members.AddRange(keyValuePair.Value);
            return members.Cast<IReadOnlyList<IMemberInfo>>();
        }

        #endregion

        #region Methods

        public void Register(IMemberInfo member, string? name = null)
        {
            Should.NotBeNull(member, nameof(member));
            if (name == null)
                name = member.Name;
            if (!_registeredMembers.TryGetValue(name, out var list))
            {
                list = new List<IMemberInfo>();
                _registeredMembers[name] = list;
            }

            list.Add(member);
            ClearCache();
        }

        public void Unregister(IMemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var removed = false;
            foreach (var pair in _registeredMembers)
            {
                if (pair.Value.Remove(member))
                    removed = true;
            }

            if (!removed)
                return;

            LazyList<KeyValuePair<TypeStringKey, object?>> valuesToUpdate = default;
            foreach (var cachePair in _cache)
            {
                var members = ItemOrList<IMemberInfo, List<IMemberInfo>>.FromRawValue(cachePair.Value);
                if (!members.Remove(member))
                    continue;
                Owner?.TryInvalidateCache(cachePair.Key.Type);
                valuesToUpdate.Add(new KeyValuePair<TypeStringKey, object?>(cachePair.Key, members.GetRawValue()));
            }

            var list = valuesToUpdate.List;
            if (list == null)
                return;
            for (var i = 0; i < list.Count; i++)
            {
                var keyValuePair = list[i];
                if (keyValuePair.Value == null)
                    _cache.Remove(keyValuePair.Key);
                else
                    _cache[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        public void Clear()
        {
            _registeredMembers.Clear();
            ClearCache();
        }

        private void ClearCache()
        {
            _cache.Clear();
            Owner.TryInvalidateCache();
        }

        #endregion
    }
}