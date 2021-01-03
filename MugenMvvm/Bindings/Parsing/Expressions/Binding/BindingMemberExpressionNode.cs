using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions.Binding
{
    public sealed class BindingMemberExpressionNode : BindingMemberExpressionNodeBase<BindingMemberExpressionNode>
    {
        #region Fields

        private MemberPathObserverRequest? _dataContextRequest;
        private MemberPathObserverRequest? _request;

        #endregion

        #region Constructors

        public BindingMemberExpressionNode(string path, int index, EnumFlags<BindingMemberExpressionFlags> flags, EnumFlags<MemberFlags> memberFlags, string? observableMethodName = null,
            IExpressionNode? expression = null, IReadOnlyDictionary<string, object?>? metadata = null) : base(path, index, flags, memberFlags, observableMethodName, expression, metadata)
        {
        }

        #endregion

        #region Methods

        public override object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path)
        {
            if (Flags.HasFlag(BindingMemberExpressionFlags.Target))
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
            if (Flags.HasFlag(BindingMemberExpressionFlags.Target))
                return GetObserver(target, metadata);
            if (source == null)
                return MugenService.ObservationManager.GetMemberPathObserver(target.ToWeakReference(), DataContextRequest(metadata), metadata);
            return GetObserver(source, metadata);
        }

        private IMemberPathObserver GetObserver(object target, IReadOnlyMetadataContext? metadata) => MugenService.ObservationManager.GetMemberPathObserver(target.ToWeakReference(), Request(metadata), metadata);

        private MemberPathObserverRequest Request(IReadOnlyMetadataContext? metadata) => _request ??= GetObserverRequest(Path, metadata);

        private MemberPathObserverRequest DataContextRequest(IReadOnlyMetadataContext? metadata) => _dataContextRequest ??= GetObserverRequest(GetDataContextPath(), metadata);

        private string GetDataContextPath()
        {
            var path = MergePath(BindableMembers.For<object>().DataContext());
            if (Flags.HasFlag(BindingMemberExpressionFlags.ParentDataContext))
                path = BindableMembers.For<object>().Parent().Name + "." + path;
            return path;
        }

        protected override BindingMemberExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new(Path, Index, Flags, MemberFlags, ObservableMethodName, Expression, metadata);

        #endregion
    }
}