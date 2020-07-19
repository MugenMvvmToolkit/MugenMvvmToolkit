using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
{
    public sealed class BindingResourceMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Fields

        private readonly IResourceResolver? _resourceResolver;
        private MemberPathObserverRequest? _request;

        #endregion

        #region Constructors

        public BindingResourceMemberExpressionNode(string resourceName, string path, IObservationManager? observationManager = null, IResourceResolver? resourceResolver = null)
            : base(path, observationManager)
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

        public override object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            path = GetMemberPath(metadata);
            memberFlags = MemberFlags;
            return GetResource(target, metadata);
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var resourceValue = GetResource(target, metadata);
            var memberPath = GetMemberPath(metadata);
            if (Flags.HasFlagEx(BindingMemberExpressionFlags.Target) || !resourceValue.IsStatic)
            {
                _request ??= GetObserverRequest(memberPath);
                return ObservationManager.DefaultIfNull().GetMemberPathObserver(resourceValue, _request, metadata);
            }

            if (resourceValue.Value == null)
                return null;
            return memberPath.GetValueFromPath(resourceValue.Value.GetType(), resourceValue.Value, MemberFlags, 1, metadata);
        }

        private IResourceValue GetResource(object target, IReadOnlyMetadataContext? metadata)
        {
            var resourceValue = _resourceResolver.DefaultIfNull().TryGetResourceValue(ResourceName, target, metadata);
            if (resourceValue == null)
                BindingExceptionManager.ThrowCannotResolveResource(ResourceName);
            return resourceValue;
        }

        #endregion
    }
}