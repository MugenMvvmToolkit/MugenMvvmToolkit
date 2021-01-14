using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestExpressionNode : ExpressionNodeBase<TestExpressionNode>
    {
        public TestExpressionNode(IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
        }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Parameter;

        public Func<IExpressionVisitor, IReadOnlyMetadataContext?, IExpressionNode?>? VisitHandler { get; set; }

        public Func<IReadOnlyDictionary<string, object?>, TestExpressionNode>? CloneHandler { get; set; }

        public Func<TestExpressionNode, TestExpressionNode, IExpressionEqualityComparer?, bool>? EqualsHandler { get; set; }

        public Func<TestExpressionNode, int, IExpressionEqualityComparer?, int>? GetHashCodeHandler { get; set; }

        public int Id { get; set; }

        public int EqualsCount { get; set; }

        public int GetHashCodeCount { get; set; }

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => VisitHandler?.Invoke(visitor, metadata) ?? this;

        protected override TestExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => CloneHandler?.Invoke(metadata) ?? this;

        protected override bool Equals(TestExpressionNode other, IExpressionEqualityComparer? comparer)
        {
            ++EqualsCount;
            return EqualsHandler?.Invoke(this, other, comparer) ?? ReferenceEquals(this, other);
        }

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer)
        {
            ++GetHashCodeCount;
            return GetHashCodeHandler?.Invoke(this, hashCode, comparer) ?? hashCode;
        }
    }
}