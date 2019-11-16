using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class SelectorMemberProviderComponent : AttachableComponentBase<IMemberProvider>, ISelectorMemberProviderComponent,
        IComponentCollectionChangedListener<IComponent<IMemberProvider>>
    {
        #region Fields

        private readonly SelectorDictionary _dictionary;

        private IMemberProviderComponent[] _memberProviders;

        #endregion

        #region Constructors

        public SelectorMemberProviderComponent()
        {
            _memberProviders = Default.EmptyArray<IMemberProviderComponent>();
            _dictionary = new SelectorDictionary();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<IMemberProvider>>.OnAdded(IComponentCollection<IComponent<IMemberProvider>> collection,
            IComponent<IMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _memberProviders, collection, component);
        }

        void IComponentCollectionChangedListener<IComponent<IMemberProvider>>.OnRemoved(IComponentCollection<IComponent<IMemberProvider>> collection,
            IComponent<IMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _memberProviders, component);
        }

        public IMemberInfo? TryGetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            var currentPriority = int.MinValue;
            IMemberInfo? currentMember = null;
            var providers = _memberProviders;
            for (var i = 0; i < providers.Length; i++)
            {
                var members = providers[i].TryGetMembers(type, name, metadata);
                for (var j = 0; j < members.Count; j++)
                {
                    var member = members[j];
                    if (!memberTypes.HasFlagEx(member.MemberType) || !flags.HasFlagEx(member.AccessModifiers))
                        continue;

                    var priority = GetPriority(member);
                    if (priority < currentPriority)
                        continue;
                    if (priority == currentPriority)
                    {
                        if (memberTypes.HasFlagEx(MemberType.Method))
                            throw new AmbiguousMatchException();
                        continue;
                    }

                    currentPriority = priority;
                    currentMember = member;
                }
            }

            return currentMember;
        }

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            _dictionary.Clear();
            var providers = _memberProviders;
            for (var i = 0; i < providers.Length; i++)
            {
                var members = providers[i].TryGetMembers(type, name, metadata);
                for (var j = 0; j < members.Count; j++)
                {
                    var member = members[j];
                    if (!memberTypes.HasFlagEx(member.MemberType) || !flags.HasFlagEx(member.AccessModifiers))
                        continue;

                    if (!_dictionary.TryGetValue(member, out var currentMember) || GetPriority(member) > GetPriority(currentMember))
                        _dictionary[member] = member;
                }
            }

            return _dictionary.KeysToArray();
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IMemberProvider owner, IReadOnlyMetadataContext? metadata)
        {
            _memberProviders = owner.Components.GetComponents().OfType<IMemberProviderComponent>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IMemberProvider owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            _memberProviders = Default.EmptyArray<IMemberProviderComponent>();
        }

        private static int GetPriority(IMemberInfo member)
        {
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Attached))
                return MemberPriority.Attached;
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Extension))
                return MemberPriority.Extension;
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Dynamic))
                return MemberPriority.Dynamic;
            return MemberPriority.Default;
        }

        #endregion

        #region Nested types

        private sealed class SelectorDictionary : LightDictionary<IMemberInfo, IMemberInfo>
        {
            #region Constructors

            public SelectorDictionary() : base(17)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(IMemberInfo key)
            {
                return key.GetHashCodeEx();
            }

            protected override bool Equals(IMemberInfo x, IMemberInfo y)
            {
                return x.EqualsEx(y);
            }

            #endregion
        }

        #endregion
    }
}