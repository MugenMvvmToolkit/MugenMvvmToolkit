using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    public class TestBindingMemberExpressionNode : ExpressionNodeBase, IBindingMemberExpressionNode
    {
        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.BindingMember;

        public BindingMemberExpressionFlags Flags { get; set; }

        public int Index { get; set; }

        public string Path { get; set; } = null!;

        public Func<object, object?, IReadOnlyMetadataContext?, (object, IMemberPath, MemberFlags)>? GetTarget { get; set; }

        public Func<object, object?, IReadOnlyMetadataContext?, (object, IMemberPath, MemberFlags)>? GetSource { get; set; }

        public Func<object, object?, IReadOnlyMetadataContext?, IMemberPathObserver>? GetBindingTarget { get; set; }

        public Func<object, object?, IReadOnlyMetadataContext?, object?>? GetBindingSource { get; set; }

        #endregion

        #region Implementation of interfaces

        object IBindingMemberExpressionNode.GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            var tuple = GetTarget?.Invoke(target, source, metadata);
            path = tuple?.Item2!;
            memberFlags = tuple?.Item3 ?? default;
            return tuple?.Item1!;
        }

        object IBindingMemberExpressionNode.GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            var tuple = GetSource?.Invoke(target, source, metadata);
            path = tuple?.Item2!;
            memberFlags = tuple?.Item3 ?? default;
            return tuple?.Item1!;
        }

        IMemberPathObserver IBindingMemberExpressionNode.GetBindingTarget(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return GetBindingTarget?.Invoke(target, source, metadata)!;
        }

        object? IBindingMemberExpressionNode.GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return GetBindingSource?.Invoke(target, source, metadata)!;
        }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            return this;
        }

        #endregion
    }
}