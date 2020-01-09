using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class AttachedDynamicRawMemberProviderComponent : AttachableComponentBase<IMemberProvider>, IAttachedRawMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly TypeStringLightDictionary<IReadOnlyList<IMemberInfo>?> _cache;
        private readonly List<Func<Type, string, IReadOnlyMetadataContext?, IMemberInfo?>> _dynamicMembers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedDynamicRawMemberProviderComponent()
        {
            _dynamicMembers = new List<Func<Type, string, IReadOnlyMetadataContext?, IMemberInfo?>>();
            _cache = new TypeStringLightDictionary<IReadOnlyList<IMemberInfo>?>(59);
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
                if (_dynamicMembers.Count != 0)
                {
                    LazyList<IMemberInfo> members = default;
                    for (var i = 0; i < _dynamicMembers.Count; i++)
                        members.AddIfNotNull(_dynamicMembers[i].Invoke(type, name, metadata));
                    list = members.List;
                }

                _cache[key] = list;
            }

            return list;
        }

        public IReadOnlyList<IMemberInfo> GetAttachedMembers(IReadOnlyMetadataContext? metadata)
        {
            return _cache.SelectMany(pair => pair.Value).ToList();
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