using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public class SynchronizedBindingMemberProvider : BindingMemberProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public SynchronizedBindingMemberProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Methods

        protected override IBindingMemberInfo? GetMemberInternal(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext? metadata)
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

        protected override IReadOnlyList<AttachedMemberRegistration> GetAttachedMembersInternal(Type type, IReadOnlyMetadataContext? metadata)
        {
            lock (TempCache)
            {
                return base.GetAttachedMembersInternal(type, metadata);
            }
        }

        #endregion
    }
}