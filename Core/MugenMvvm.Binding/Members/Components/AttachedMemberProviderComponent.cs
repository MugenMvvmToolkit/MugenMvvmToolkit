using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
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
    public sealed class AttachedMemberProviderComponent : AttachableComponentBase<IMemberProvider>, IAttachedMemberProviderComponent, IEqualityComparer<IMemberInfo>, IHasPriority
    {
        #region Fields

        private readonly TypeStringLightDictionary<List<IMemberInfo>?> _cache;
        private readonly StringOrdinalLightDictionary<HashSet<IMemberInfo>> _registeredMembers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedMemberProviderComponent()
        {
            _registeredMembers = new StringOrdinalLightDictionary<HashSet<IMemberInfo>>(59);
            _cache = new TypeStringLightDictionary<List<IMemberInfo>?>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Attached;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var key = new TypeStringKey(type, name);
            if (!_cache.TryGetValue(key, out var list))
            {
                if (_registeredMembers.TryGetValue(name, out var set))
                {
                    LazyList<IMemberInfo> result = default;
                    foreach (var memberInfo in set)
                    {
                        if (memberInfo.DeclaringType.IsAssignableFromGeneric(type))
                            result.Add(memberInfo);
                    }

                    list = result;
                }

                _cache[key] = list;
            }

            return list;
        }

        public IReadOnlyList<IMemberInfo> GetAttachedMembers(IReadOnlyMetadataContext? metadata)
        {
            return _registeredMembers.SelectMany(pair => pair.Value).ToList();
        }

        bool IEqualityComparer<IMemberInfo>.Equals(IMemberInfo x, IMemberInfo y)
        {
            if (x.MemberType != y.MemberType || x.Name != y.Name || x.DeclaringType != y.DeclaringType)
                return false;

            if (x.MemberType != MemberType.Method)
                return true;

            var xM = ((IMethodInfo)x).GetParameters();
            var yM = ((IMethodInfo)y).GetParameters();
            if (xM.Count != yM.Count)
                return false;

            for (var i = 0; i < xM.Count; i++)
            {
                if (xM[i].ParameterType != yM[i].ParameterType)
                    return false;
            }

            return true;
        }

        int IEqualityComparer<IMemberInfo>.GetHashCode(IMemberInfo obj)
        {
            return HashCode.Combine(obj.DeclaringType, (int)obj.MemberType, obj.Name);
        }

        #endregion

        #region Methods

        public void Register(IMemberInfo member, string? name = null)
        {
            Should.NotBeNull(member, nameof(member));
            AddMember(name ?? member.Name, member);
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

            foreach (var cachePair in _cache)
            {
                if (cachePair.Value != null && cachePair.Value.Remove(member))
                    Owner?.TryInvalidateCache(cachePair.Key.Type);
            }
        }

        public void Unregister(Type? type = null, string? name = null, MemberType memberType = MemberType.All)
        {
            foreach (var pair in _registeredMembers)
            {
                if (name != null && pair.Key != name)
                    continue;

                foreach (var member in pair.Value)
                {
                    if (!memberType.HasFlagEx(member.MemberType))
                        continue;

                    if (type != null && member.DeclaringType != type)
                        continue;

                    foreach (var cachePair in _cache)
                    {
                        if (cachePair.Value != null && cachePair.Value.Remove(member))
                            Owner.TryInvalidateCache(cachePair.Key.Type);
                    }
                }
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

        private void AddMember(string key, IMemberInfo member)
        {
            if (!_registeredMembers.TryGetValue(key, out var list))
            {
                list = new HashSet<IMemberInfo>(this);
                _registeredMembers[key] = list;
            }

            list.Remove(member);
            list.Add(member);
        }

        #endregion
    }
}