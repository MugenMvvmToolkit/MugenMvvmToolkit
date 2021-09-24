using System.Collections.Generic;
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
        private MemberPathObserverRequest? _request;

        public BindingInstanceMemberExpressionNode(object? instance, string path, int index, EnumFlags<BindingMemberExpressionFlags> flags, EnumFlags<MemberFlags> memberFlags,
            string? observableMethodName = null, IExpressionNode? expression = null, IReadOnlyDictionary<string, object?>? metadata = null)
            : base(path, index, flags, memberFlags, observableMethodName, expression, metadata)
        {
            Instance = instance;
        }

        public object? Instance { get; }

        public override object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path)
        {
            if (string.IsNullOrEmpty(Path))
            {
                path = MemberPath.Empty;
                if (Instance is IConstantExpressionNode constantExpressionNode)
                    return constantExpressionNode.Value;
            }
            else
                path = Request(metadata).Path;

            return Instance;
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata) =>
            Instance == null || string.IsNullOrEmpty(Path) ? Instance : MugenService.ObservationManager.GetMemberPathObserver(Instance, Request(metadata), metadata);

        protected override BindingInstanceMemberExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) =>
            new(Instance, Path, Index, Flags, MemberFlags, ObservableMethodName, Expression, metadata);

        protected override bool Equals(BindingInstanceMemberExpressionNode other, IExpressionEqualityComparer? comparer) =>
            Equals(Instance, other.Instance) && base.Equals(other, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => base.GetHashCode((hashCode * 397) ^ (Instance?.GetHashCode() ?? 0), comparer);

        private MemberPathObserverRequest Request(IReadOnlyMetadataContext? metadata) => _request ??= GetObserverRequest(Path, metadata);
    }
}