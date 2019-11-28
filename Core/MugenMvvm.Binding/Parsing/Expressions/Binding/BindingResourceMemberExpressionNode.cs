using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
{
    public sealed class BindingResourceMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Fields

        private readonly IResourceResolver? _resourceResolver;

        #endregion

        #region Constructors

        public BindingResourceMemberExpressionNode(string resourceName, string path, IObserverProvider? observerProvider = null, IResourceResolver? resourceResolver = null)
            : base(path, observerProvider)
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

        public override object GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            path = GetMemberPath(metadata);
            memberFlags = MemberFlags;
            return GetResource(metadata);
        }

        public override object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            path = GetMemberPath(metadata);
            memberFlags = MemberFlags;
            return GetResource(metadata);
        }

        public override IMemberPathObserver GetBindingTarget(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return GetObserver(GetResource(metadata), GetMemberPath(metadata), metadata);
        }

        public override object GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return GetObserver(GetResource(metadata), GetMemberPath(metadata), metadata);
        }

        private IResourceValue GetResource(IReadOnlyMetadataContext? metadata)
        {
            var resourceValue = _resourceResolver.DefaultIfNull().TryGetResourceValue(ResourceName, metadata);
            if (resourceValue == null)
                BindingExceptionManager.ThrowCannotResolveResource(ResourceName);
            return resourceValue;
        }

        #endregion
    }
}