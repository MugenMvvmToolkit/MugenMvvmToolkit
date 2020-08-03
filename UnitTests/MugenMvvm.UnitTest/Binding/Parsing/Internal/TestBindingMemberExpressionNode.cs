using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    public class TestBindingMemberExpressionNode : ExpressionNodeBase, IBindingMemberExpressionNode
    {
        #region Constructors

        public TestBindingMemberExpressionNode(string? path = null)
        {
            Path = path!;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.BindingMember;

        public BindingMemberExpressionFlags Flags { get; set; }

        public MemberFlags MemberFlags { get; set; }

        public int Index { get; set; }

        public string Path { get; set; }

        public Func<object, object?, IReadOnlyMetadataContext?, (object, IMemberPath, MemberFlags)>? GetSource { get; set; }

        public Func<object, object?, IReadOnlyMetadataContext?, object?>? GetBindingSource { get; set; }

        public Func<IExpressionVisitor, IReadOnlyMetadataContext?, IExpressionNode?>? VisitHandler { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IBindingMemberExpressionNode.GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags)
        {
            var tuple = GetSource?.Invoke(target, source, metadata);
            path = tuple?.Item2!;
            memberFlags = tuple?.Item3 ?? default;
            return tuple?.Item1!;
        }

        object? IBindingMemberExpressionNode.GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return GetBindingSource?.Invoke(target, source, metadata)!;
        }

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            return VisitHandler?.Invoke(visitor, metadata) ?? this;
        }

        public override string ToString()
        {
            return Path ?? base.ToString()!;
        }

        #endregion
    }
}