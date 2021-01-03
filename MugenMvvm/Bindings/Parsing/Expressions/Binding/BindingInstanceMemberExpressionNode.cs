﻿using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions.Binding
{
    public sealed class BindingInstanceMemberExpressionNode : BindingMemberExpressionNodeBase<BindingInstanceMemberExpressionNode>
    {
        #region Fields

        private MemberPathObserverRequest? _request;

        #endregion

        #region Constructors

        public BindingInstanceMemberExpressionNode(object instance, string path, int index, EnumFlags<BindingMemberExpressionFlags> flags, EnumFlags<MemberFlags> memberFlags, string? observableMethodName = null,
            IExpressionNode? expression = null, IReadOnlyDictionary<string, object?>? metadata = null) : base(path, index, flags, memberFlags, observableMethodName, expression, metadata)
        {
            Should.NotBeNull(instance, nameof(instance));
            Instance = instance;
        }

        #endregion

        #region Properties

        public object Instance { get; }

        #endregion

        #region Methods

        public override object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path)
        {
            path = Request(metadata).Path;
            return Instance;
        }

        public override object GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata) => MugenService.ObservationManager.GetMemberPathObserver(Instance, Request(metadata), metadata);

        private MemberPathObserverRequest Request(IReadOnlyMetadataContext? metadata) => _request ??= GetObserverRequest(Path, metadata);

        protected override BindingInstanceMemberExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new(Instance, Path, Index, Flags, MemberFlags, ObservableMethodName, Expression, metadata);

        protected override bool Equals(BindingInstanceMemberExpressionNode other, IExpressionEqualityComparer? comparer) => Instance.Equals(other.Instance) && base.Equals(other, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => base.GetHashCode(hashCode * 397 ^ Instance.GetHashCode(), comparer);

        #endregion
    }
}