using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions.Binding
{
    public sealed class BindingMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Fields

        private MemberPathObserverRequest? _dataContextRequest;
        private MemberPathObserverRequest? _request;

        #endregion

        #region Constructors

        public BindingMemberExpressionNode(string path, IObservationManager? observationManager = null) : base(path, observationManager)
        {
        }

        #endregion

        #region Methods

        public override object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            memberFlags = MemberFlags;
            if (Flags.HasFlagEx(BindingMemberExpressionFlags.Target))
            {
                path = Request(metadata).Path;
                return target;
            }

            if (source == null)
            {
                path = DataContextRequest(metadata).Path;
                return target;
            }

            path = Request(metadata).Path;
            return source;
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (Flags.HasFlagEx(BindingMemberExpressionFlags.Target))
                return GetObserver(target, metadata);
            if (source == null)
                return ObservationManager.DefaultIfNull().GetMemberPathObserver(target.ToWeakReference(), DataContextRequest(metadata), metadata);
            return GetObserver(source, metadata);
        }

        private IMemberPathObserver GetObserver(object target, IReadOnlyMetadataContext? metadata) => ObservationManager.DefaultIfNull().GetMemberPathObserver(target.ToWeakReference(), Request(metadata), metadata);

        private MemberPathObserverRequest Request(IReadOnlyMetadataContext? metadata) => _request ??= GetObserverRequest(Path, metadata);

        private MemberPathObserverRequest DataContextRequest(IReadOnlyMetadataContext? metadata) => _dataContextRequest ??= GetObserverRequest(GetDataContextPath(), metadata);

        private string GetDataContextPath()
        {
            var path = MergePath(BindableMembers.For<object>().DataContext());
            if (Flags.HasFlagEx(BindingMemberExpressionFlags.DataContextPath))
                path = BindableMembers.For<object>().Parent().Name + "." + path;
            return path;
        }

        #endregion
    }
}