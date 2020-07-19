using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
{
    public sealed class BindingInstanceMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Fields

        private MemberPathObserverRequest? _request;

        #endregion

        #region Constructors

        public BindingInstanceMemberExpressionNode(object instance, string path, IObservationManager? observationManager = null)
            : base(path, observationManager)
        {
            Should.NotBeNull(instance, nameof(instance));
            Instance = instance;
        }

        #endregion

        #region Properties

        public object Instance { get; }

        #endregion

        #region Methods

        public override object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            path = GetMemberPath(metadata);
            memberFlags = MemberFlags;
            return Instance;
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            _request ??= GetObserverRequest(GetMemberPath(metadata));
            return ObservationManager.DefaultIfNull().GetMemberPathObserver(Instance, _request, metadata);
        }

        #endregion
    }
}