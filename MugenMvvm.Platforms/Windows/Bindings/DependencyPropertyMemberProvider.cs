using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Windows.Bindings
{
    public sealed class DependencyPropertyMemberProvider : IMemberProviderComponent, IEqualityComparer<(Type, string)>, IHasPriority
    {
        private readonly Dictionary<(Type, string), DependencyPropertyAccessorMemberInfo?> _members;

        public DependencyPropertyMemberProvider()
        {
            _members = new Dictionary<(Type, string), DependencyPropertyAccessorMemberInfo?>(59, this);
        }

        public int Priority { get; set; } = MemberComponentPriority.Instance;

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlag(MemberType.Accessor))
                return default;

            var key = (type, name);
            if (!_members.TryGetValue(key, out var v))
            {
                var p = DependencyPropertyDescriptor.FromName(name, type, type)?.DependencyProperty;
                v = p == null ? null : new DependencyPropertyAccessorMemberInfo(p, name, type, MemberFlags.InstancePublic);
                _members[key] = v;
            }

            return v;
        }

        bool IEqualityComparer<(Type, string)>.Equals((Type, string) x, (Type, string) y) => x.Item1 == y.Item1 && x.Item2 == y.Item2;

        int IEqualityComparer<(Type, string)>.GetHashCode((Type, string) obj) => HashCode.Combine(obj.Item1, obj.Item2);
    }
}