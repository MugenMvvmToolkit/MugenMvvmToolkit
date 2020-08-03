using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
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
                return ObservationManager.DefaultIfNull().GetMemberPathObserver(target, DataContextRequest(metadata), metadata);
            return GetObserver(source, metadata);
        }

        private IMemberPathObserver GetObserver(object target, IReadOnlyMetadataContext? metadata)
        {
            return ObservationManager.DefaultIfNull().GetMemberPathObserver(target, Request(metadata), metadata);
        }

        private MemberPathObserverRequest Request(IReadOnlyMetadataContext? metadata)
        {
            return _request ??= GetObserverRequest(Path, metadata);
        }

        private MemberPathObserverRequest DataContextRequest(IReadOnlyMetadataContext? metadata)
        {
            return _dataContextRequest ??= GetObserverRequest(GetDataContextPath(), metadata);
        }

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