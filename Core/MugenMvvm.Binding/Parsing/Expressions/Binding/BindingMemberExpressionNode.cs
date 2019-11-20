using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
{
    public sealed class BindingMemberExpressionNode : BindingMemberExpressionNodeBase
    {
        #region Fields

        private readonly TargetType _targetType;
        private IMemberPath? _dataContextMemberPath;

        #endregion

        #region Constructors

        public BindingMemberExpressionNode(TargetType targetType, string path, IObserverProvider? observerProvider) : base(path, observerProvider)
        {
            _targetType = targetType;
        }

        #endregion

        #region Methods

        public override object GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            memberFlags = MemberFlags;
            path = GetMemberPath(metadata);
            if (_targetType == TargetType.SourceOnly)
                return source ?? target;
            return target;
        }

        public override object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            memberFlags = MemberFlags;
            if (_targetType == TargetType.Default && source == null)
            {
                path = GetDataContextPath(metadata);
                return target;
            }

            path = GetMemberPath(metadata);
            if (_targetType == TargetType.TargetOnly)
                return target;
            return source ?? target;
        }

        public override IMemberPathObserver GetTargetObserver(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (_targetType == TargetType.SourceOnly && source != null)
                target = source;
            return GetObserver(target, GetMemberPath(metadata), metadata);
        }

        public override IMemberPathObserver GetSourceObserver(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            IMemberPath path;
            if (_targetType == TargetType.Default && source == null)
                path = GetDataContextPath(metadata);
            else
            {
                path = GetMemberPath(metadata);
                if (_targetType != TargetType.TargetOnly && source != null)
                    target = source;
            }

            return GetObserver(target, path, metadata);
        }

        private IMemberPath GetDataContextPath(IReadOnlyMetadataContext? metadata)
        {
            if (_dataContextMemberPath != null)
                return _dataContextMemberPath;

            string path;
            if (Name.StartsWith("[", StringComparison.Ordinal))
                path = BindableMembers.Object.DataContext + Name;
            else
                path = BindableMembers.Object.DataContext + "." + Name;
            _dataContextMemberPath = ObserverProvider.DefaultIfNull().GetMemberPath(path, metadata);
            return _dataContextMemberPath;
        }

        #endregion

        #region Nested types

        public enum TargetType : byte
        {
            Default = 0,
            TargetOnly = 1,
            SourceOnly = 2
        }

        #endregion
    }
}