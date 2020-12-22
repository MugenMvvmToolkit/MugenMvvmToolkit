using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions.Binding
{
    public sealed class BindingInstanceMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Fields

        private MemberPathObserverRequest? _request;

        #endregion

        #region Constructors

        public BindingInstanceMemberExpressionNode(object instance, string path, IObservationManager? observationManager = null, IDictionary<string, object?>? metadata = null)
            : base(path, observationManager, metadata)
        {
            Should.NotBeNull(instance, nameof(instance));
            Instance = instance;
        }

        #endregion

        #region Properties

        public object Instance { get; }

        #endregion

        #region Methods

        public override object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path)
        {
            path = Request(metadata).Path;
            return Instance;
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata) => ObservationManager.DefaultIfNull().GetMemberPathObserver(Instance, Request(metadata), metadata);

        private MemberPathObserverRequest Request(IReadOnlyMetadataContext? metadata) => _request ??= GetObserverRequest(Path, metadata);

        #endregion
    }
}