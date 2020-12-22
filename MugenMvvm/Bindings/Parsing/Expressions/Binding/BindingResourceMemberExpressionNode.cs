using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions.Binding
{
    public sealed class BindingResourceMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Fields

        private readonly IResourceResolver? _resourceResolver;
        private MemberPathObserverRequest? _request;
        private MemberPathObserverRequest? _requestResource;

        #endregion

        #region Constructors

        public BindingResourceMemberExpressionNode(string resourceName, string path, IObservationManager? observationManager = null,
            IResourceResolver? resourceResolver = null, IDictionary<string, object?>? metadata = null)
            : base(path, observationManager, metadata)
        {
            Should.NotBeNull(resourceName, nameof(resourceName));
            ResourceName = resourceName;
            _resourceResolver = resourceResolver;
        }

        #endregion

        #region Properties

        public string ResourceName { get; }

        #endregion

        #region Methods

        public override object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path)
        {
            var resource = GetResource(target, metadata, out var r);
            path = r.Path;
            return resource;
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var resource = GetResource(target, metadata, out var request);
            if (_request == request && !Flags.HasFlag(BindingMemberExpressionFlags.Target) && string.IsNullOrEmpty(Path))
                return resource;

            if (resource == null)
                ExceptionManager.ThrowCannotResolveResource(ResourceName);

            return ObservationManager.DefaultIfNull().GetMemberPathObserver(resource, request, metadata);
        }

        private object? GetResource(object target, IReadOnlyMetadataContext? metadata, out MemberPathObserverRequest request)
        {
            var resource = _resourceResolver.DefaultIfNull().TryGetResource(ResourceName, target, metadata);
            if (!resource.IsResolved)
                ExceptionManager.ThrowCannotResolveResource(ResourceName);

            if (resource.Resource is IDynamicResource)
            {
                _requestResource ??= GetObserverRequest(MergePath(nameof(IDynamicResource.Value)), metadata);
                request = _requestResource;
            }
            else
            {
                _request ??= GetObserverRequest(Path, metadata);
                request = _request;
            }

            return resource.Resource;
        }

        #endregion
    }
}