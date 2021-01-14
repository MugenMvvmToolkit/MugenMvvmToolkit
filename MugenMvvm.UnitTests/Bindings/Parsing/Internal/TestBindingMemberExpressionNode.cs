using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestBindingMemberExpressionNode : ExpressionNodeBase<TestBindingMemberExpressionNode>, IBindingMemberExpressionNode
    {
        public TestBindingMemberExpressionNode(string? path = null, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Path = path!;
        }

        public Func<object, object?, IReadOnlyMetadataContext?, (object, IMemberPath)>? GetSource { get; set; }

        public Func<object, object?, IReadOnlyMetadataContext?, object?>? GetBindingSource { get; set; }

        public Func<IExpressionVisitor, IReadOnlyMetadataContext?, IExpressionNode?>? VisitHandler { get; set; }

        public Func<IReadOnlyDictionary<string, object?>, TestBindingMemberExpressionNode>? CloneHandler { get; set; }

        public Func<TestBindingMemberExpressionNode, IExpressionEqualityComparer?, bool>? EqualsHandler { get; set; }

        public Func<int, IExpressionEqualityComparer?, int>? GetHashCodeHandler { get; set; }

        public Func<int, EnumFlags<BindingMemberExpressionFlags>, EnumFlags<MemberFlags>, string?, IBindingMemberExpressionNode>? Update { get; set; }

        public EnumFlags<BindingMemberExpressionFlags> Flags { get; set; }

        public EnumFlags<MemberFlags> MemberFlags { get; set; }

        public int Index { get; set; }

        public string Path { get; set; }

        public string? ObservableMethodName { get; set; }

        public IExpressionNode? Expression { get; set; }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.BindingParameter;

        public override string ToString() => Path ?? base.ToString()!;

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => VisitHandler?.Invoke(visitor, metadata) ?? this;

        protected override TestBindingMemberExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => CloneHandler?.Invoke(metadata) ?? this;

        protected override bool Equals(TestBindingMemberExpressionNode other, IExpressionEqualityComparer? comparer)
        {
            if (EqualsHandler == null)
                return Path == other.Path && Index == other.Index;
            return EqualsHandler.Invoke(other, comparer);
        }

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => GetHashCodeHandler?.Invoke(hashCode, comparer) ?? hashCode;

        object? IBindingMemberExpressionNode.GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path)
        {
            var tuple = GetSource?.Invoke(target, source, metadata);
            path = tuple?.Item2!;
            return tuple?.Item1!;
        }

        object? IBindingMemberExpressionNode.GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata) =>
            GetBindingSource?.Invoke(target, source, metadata)!;

        IBindingMemberExpressionNode IBindingMemberExpressionNode.Update(int index, EnumFlags<BindingMemberExpressionFlags> flags, EnumFlags<MemberFlags> memberFlags,
            string? observableMethodName) =>
            Update?.Invoke(index, flags, memberFlags, observableMethodName) ?? this;
    }
}