using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Members
{
    public class SynchronizedBindingMemberProvider : BindingMemberProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public SynchronizedBindingMemberProvider(IAttachedChildBindingMemberProvider attachedChildBindingMemberProvider, IComponentCollectionProvider componentCollectionProvider) : base(
            attachedChildBindingMemberProvider, componentCollectionProvider)
        {
        }

        #endregion

        #region Methods

        protected override IBindingMemberInfo GetMemberInternal(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext metadata)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(TempCache, ref lockTaken);
                if (!CurrentNames.Add(name))
                    return null;

                return GetMemberImpl(type, name, ignoreAttachedMembers, metadata);
            }
            finally
            {
                CurrentNames.Remove(name);
                if (lockTaken)
                    Monitor.Exit(TempCache);
            }
        }

        protected override IReadOnlyList<AttachedMemberRegistration> GetAttachedMembersInternal(Type type, IReadOnlyMetadataContext metadata)
        {
            lock (TempCache)
            {
                return base.GetAttachedMembersInternal(type, metadata);
            }
        }

        protected override void RegisterInternal(Type type, IBindingMemberInfo member, string name, IReadOnlyMetadataContext metadata)
        {
            lock (TempCache)
            {
                base.RegisterInternal(type, member, name, metadata);
            }
        }

        protected override bool UnregisterInternal(Type type, string name, IReadOnlyMetadataContext metadata)
        {
            lock (TempCache)
            {
                return base.UnregisterInternal(type, name, metadata);
            }
        }

        #endregion
    }
}