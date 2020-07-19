using System;
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

        private IMemberPath? _dataContextMemberPath;
        private MemberPathObserverRequest? _dataContextRequest;
        private MemberPathObserverRequest? _request;

        #endregion

        #region Constructors

        public BindingMemberExpressionNode(string path, IObservationManager? observationManager = null) : base(path, observationManager)
        {
        }

        #endregion

        #region Methods

        public override object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            memberFlags = MemberFlags;
            if (Flags.HasFlagEx(BindingMemberExpressionFlags.Target))
            {
                path = GetMemberPath(metadata);
                return target;
            }

            if (source == null)
            {
                path = GetDataContextPath(metadata);
                return target;
            }

            path = GetMemberPath(metadata);
            return source;
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (Flags.HasFlagEx(BindingMemberExpressionFlags.Target))
                return GetObserver(target, metadata);
            if (source == null)
                return GetDataContextObserver(target, metadata);
            return GetObserver(source, metadata);
        }

        private IMemberPathObserver GetObserver(object target, IReadOnlyMetadataContext? metadata)
        {
            _request ??= GetObserverRequest(GetMemberPath(metadata));
            return ObservationManager.DefaultIfNull().GetMemberPathObserver(target, _request, metadata);
        }

        private IMemberPathObserver GetDataContextObserver(object target, IReadOnlyMetadataContext? metadata)
        {
            _dataContextRequest ??= GetObserverRequest(GetDataContextPath(metadata));
            return ObservationManager.DefaultIfNull().GetMemberPathObserver(target, _dataContextRequest, metadata);
        }

        private IMemberPath GetDataContextPath(IReadOnlyMetadataContext? metadata)
        {
            if (_dataContextMemberPath != null)
                return _dataContextMemberPath;

            string path;
            if (string.IsNullOrEmpty(Path))
                path = BindableMembers.For<object>().DataContext();
            else if (Path.StartsWith("[", StringComparison.Ordinal))
                path = BindableMembers.For<object>().DataContext().Name + Path;
            else
                path = BindableMembers.For<object>().DataContext().Name + "." + Path;
            if (Flags.HasFlagEx(BindingMemberExpressionFlags.DataContextPath))
                path = BindableMembers.For<object>().Parent().Name + "." + path;
            _dataContextMemberPath = ObservationManager.DefaultIfNull().GetMemberPath(path, metadata);
            return _dataContextMemberPath;
        }

        #endregion
    }
}