using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
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

        public BindingMemberExpressionNode(TargetType targetType, string path, IObserverProvider? observerProvider = null) : base(path, observerProvider)
        {
            Type = targetType;
        }

        #endregion

        #region Properties

        public TargetType Type { get; }

        #endregion

        #region Methods

        public override object GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            memberFlags = MemberFlags;
            path = GetMemberPath(metadata);
            if (Type == TargetType.SourceOnly)
                return source ?? target;
            return target;
        }

        public override object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            memberFlags = MemberFlags;
            if (Type == TargetType.Default && source == null)
            {
                path = GetDataContextPath(metadata);
                return target;
            }

            path = GetMemberPath(metadata);
            if (Type == TargetType.TargetOnly)
                return target;
            return source ?? target;
        }

        public override IMemberPathObserver GetBindingTarget(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (Type == TargetType.SourceOnly && source != null)
                target = source;
            return GetObserver(target, GetMemberPath(metadata), metadata);
        }

        public override object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            IMemberPath path;
            if (Type == TargetType.Default && source == null)
                path = GetDataContextPath(metadata);
            else
            {
                path = GetMemberPath(metadata);
                if (Type != TargetType.TargetOnly && source != null)
                    target = source;
            }

            return GetObserver(target, path, metadata);
        }

        private IMemberPath GetDataContextPath(IReadOnlyMetadataContext? metadata)
        {
            if (_dataContextMemberPath != null)
                return _dataContextMemberPath;

            string path;
            if (Path.StartsWith("[", StringComparison.Ordinal))
                path = BindableMembers.Object.DataContext + Path;
            else
                path = BindableMembers.Object.DataContext + "." + Path;
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