using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
{
    public sealed class BindingMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Fields

        private IMemberPath? _dataContextMemberPath;

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
                return GetObserver(target, GetMemberPath(metadata), metadata);
            if (source == null)
                return GetObserver(target, GetDataContextPath(metadata), metadata);
            return GetObserver(source, GetMemberPath(metadata), metadata);
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