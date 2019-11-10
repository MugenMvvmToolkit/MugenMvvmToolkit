using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class SelectorMemberProviderComponent : AttachableComponentBase<IMemberProvider>, ISelectorMemberProviderComponent,
        IComponentCollectionChangedListener<IComponent<IMemberProvider>>
    {
        #region Fields

        private readonly List<KeyValuePair<Type, IMemberInfo>> _buffer;
        private IMemberProviderComponent[] _memberProviders;

        #endregion

        #region Constructors

        public SelectorMemberProviderComponent()
        {
            _buffer = new List<KeyValuePair<Type, IMemberInfo>>(8);
            _memberProviders = Default.EmptyArray<IMemberProviderComponent>();
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
            _buffer.Clear();
            int currentPriority = MemberPriority.Default;
            for (int i = 0; i < _memberProviders.Length; i++)
            {
                var members = _memberProviders[i].TryGetMembers(type, name, metadata);
                for (int j = 0; j < members.Count; j++)
                {
                    var member = members[j];
                    if (!memberTypes.HasFlagEx(member.MemberType) || !flags.HasFlagEx(member.AccessModifiers))
                        continue;

                    var priority = GetPriority(member);
                    if (priority > currentPriority)
                    {
                        _buffer.Clear();
                        currentPriority = priority;
                    }
                    else if (priority < currentPriority)
                        continue;

                    _buffer.Add(new KeyValuePair<Type, IMemberInfo>(member.DeclaringType, member));
                }
            }

            return MugenBindingExtensions.FindBestMember(_buffer);
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
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Dynamic))
                return MemberPriority.Dynamic;
            return MemberPriority.Default;
        }

        #endregion
    }
}