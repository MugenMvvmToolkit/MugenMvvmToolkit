using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Internal;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class AttachedDynamicMemberProviderComponent : AttachableComponentBase<IMemberManager>, IAttachedMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly TypeStringLightDictionary<object?> _cache;
        private readonly List<Func<Type, string, IReadOnlyMetadataContext?, IMemberInfo?>> _dynamicMembers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedDynamicMemberProviderComponent()
        {
            _dynamicMembers = new List<Func<Type, string, IReadOnlyMetadataContext?, IMemberInfo?>>();
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
            if (!_cache.TryGetValue(key, out var result))
            {
                if (_dynamicMembers.Count != 0)
                {
                    ItemOrList<IMemberInfo, List<IMemberInfo>> members = default;
                    for (var i = 0; i < _dynamicMembers.Count; i++)
                    {
                        var value = _dynamicMembers[i].Invoke(type, name, metadata);
                        if (value != null)
                            members.Add(value);
                    }
                    result = members.GetRawValue();
                }

                _cache[key] = result;
            }

            return ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>.FromRawValue(result);
        }

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> GetAttachedMembers(IReadOnlyMetadataContext? metadata)
        {
            ItemOrList<IMemberInfo, List<IMemberInfo>> members = default;
            foreach (var keyValuePair in _cache)
                members.AddRange(ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>.FromRawValue(keyValuePair.Value));
            return members.Cast<IReadOnlyList<IMemberInfo>>();
        }

        #endregion

        #region Methods

        public void Register(Func<Type, string, IReadOnlyMetadataContext?, IMemberInfo?> getMember)
        {
            Should.NotBeNull(getMember, nameof(getMember));
            _dynamicMembers.Add(getMember);
            ClearCache();
        }

        public void Unregister(Func<Type, string, IReadOnlyMetadataContext?, IMemberInfo?> getMember)
        {
            Should.NotBeNull(getMember, nameof(getMember));
            _dynamicMembers.Remove(getMember);
            ClearCache();
        }

        public void Clear()
        {
            _dynamicMembers.Clear();
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