using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions.Binding
{
    public sealed class BindingResourceMemberExpressionNode : BindingMemberExpressionNodeBase<BindingResourceMemberExpressionNode>
    {
        private MemberPathObserverRequest? _request;
        private MemberPathObserverRequest? _requestResource;

        public BindingResourceMemberExpressionNode(string resourceName, string path, int index, EnumFlags<BindingMemberExpressionFlags> flags, EnumFlags<MemberFlags> memberFlags,
            string? observableMethodName = null,
            IExpressionNode? expression = null, IReadOnlyDictionary<string, object?>? metadata = null) : base(path, index, flags, memberFlags, observableMethodName, expression,
            metadata)
        {
            Should.NotBeNull(resourceName, nameof(resourceName));
            ResourceName = resourceName;
        }

        public string ResourceName { get; }

        public override object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path)
        {
            var resource = GetResource(target, metadata, out var r);
            path = r?.Path ?? MemberPath.Empty;
            return resource;
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var resource = GetResource(target, metadata, out var request);
            if (request == null)
                return resource;

            if (resource == null)
                ExceptionManager.ThrowCannotResolveResource(ResourceName);

            return MugenService.ObservationManager.GetMemberPathObserver(resource, request, metadata);
        }

        protected override bool Equals(BindingResourceMemberExpressionNode other, IExpressionEqualityComparer? comparer) =>
            ResourceName.Equals(other.ResourceName) && base.Equals(other, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => base.GetHashCode((hashCode * 397) ^ ResourceName.GetHashCode(), comparer);

        protected override BindingResourceMemberExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) =>
            new(ResourceName, Path, Index, Flags, MemberFlags, ObservableMethodName, Expression, metadata);

        private object? GetResource(object target, IReadOnlyMetadataContext? metadata, out MemberPathObserverRequest? request)
        {
            var resource = MugenService.ResourceManager.TryGetResource(ResourceName, target, metadata);
            if (!resource.IsResolved)
                ExceptionManager.ThrowCannotResolveResource(ResourceName);

            if (resource.Resource is IDynamicResource)
            {
                _requestResource ??= GetObserverRequest(MergePath(nameof(IDynamicResource.Value)), metadata);
                request = _requestResource;
            }
            else
            {
                if (!Flags.HasFlag(BindingMemberExpressionFlags.Target) && string.IsNullOrEmpty(Path))
                    request = null;
                else
                {
                    _request ??= GetObserverRequest(Path, metadata);
                    request = _request;
                }
            }

            return resource.Resource;
        }
    }
}