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
    public class TestBindingMemberExpressionNode : ExpressionNodeBase, IBindingMemberExpressionNode
    {
        #region Constructors

        public TestBindingMemberExpressionNode(string? path = null, IDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Path = path!;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.BindingMember;

        public EnumFlags<BindingMemberExpressionFlags> Flags { get; set; }

        public EnumFlags<MemberFlags> MemberFlags { get; set; }

        public int Index { get; set; }

        public string Path { get; set; }
        
        public IExpressionNode? OriginalExpression { get; set; }

        public Func<object, object?, IReadOnlyMetadataContext?, (object, IMemberPath)>? GetSource { get; set; }

        public Func<object, object?, IReadOnlyMetadataContext?, object?>? GetBindingSource { get; set; }

        public Func<IExpressionVisitor, IReadOnlyMetadataContext?, IExpressionNode?>? VisitHandler { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IBindingMemberExpressionNode.GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path)
        {
            var tuple = GetSource?.Invoke(target, source, metadata);
            path = tuple?.Item2!;
            return tuple?.Item1!;
        }

        object? IBindingMemberExpressionNode.GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata) => GetBindingSource?.Invoke(target, source, metadata)!;

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => VisitHandler?.Invoke(visitor, metadata) ?? this;

        public override string ToString() => Path ?? base.ToString()!;

        #endregion
    }
}