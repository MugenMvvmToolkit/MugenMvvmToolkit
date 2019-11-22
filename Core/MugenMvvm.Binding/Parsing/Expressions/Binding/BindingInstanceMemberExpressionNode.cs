﻿using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
{
    public sealed class BindingInstanceMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Constructors

        public BindingInstanceMemberExpressionNode(object instance, string path, IObserverProvider? observerProvider = null)
            : base(path, observerProvider)
        {
            Should.NotBeNull(instance, nameof(instance));
            Instance = instance;
        }

        #endregion

        #region Properties

        public object Instance { get; }

        #endregion

        #region Methods

        public override object GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            path = GetMemberPath(metadata);
            memberFlags = MemberFlags;
            return Instance;
        }

        public override object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            path = GetMemberPath(metadata);
            memberFlags = MemberFlags;
            return Instance;
        }

        public override IMemberPathObserver GetTargetObserver(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return GetObserver(Instance, GetMemberPath(metadata), metadata);
        }

        public override IMemberPathObserver GetSourceObserver(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return GetObserver(Instance, GetMemberPath(metadata), metadata);
        }

        #endregion
    }
}