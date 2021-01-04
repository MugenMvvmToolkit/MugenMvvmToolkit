using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions.Binding
{
    public abstract class BindingMemberExpressionNodeBase<TExpression> : ExpressionNodeBase<TExpression>, IBindingMemberExpressionNode
        where TExpression : BindingMemberExpressionNodeBase<TExpression>
    {
        #region Fields

        private ushort _flags;
        private ushort _memberFlags;

        #endregion

        #region Constructors

        protected BindingMemberExpressionNodeBase(string path, int index, EnumFlags<BindingMemberExpressionFlags> flags, EnumFlags<MemberFlags> memberFlags, string? observableMethodName,
            IExpressionNode? expression, IReadOnlyDictionary<string, object?>? metadata) : base(metadata)
        {
            Should.NotBeNull(path, nameof(path));
            _flags = flags.Value();
            _memberFlags = memberFlags.Value();
            Path = path;
            Index = index;
            Expression = expression;
            ObservableMethodName = observableMethodName;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.BindingParameter;

        public EnumFlags<BindingMemberExpressionFlags> Flags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_flags);
        }

        public EnumFlags<MemberFlags> MemberFlags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_memberFlags);
        }

        public int Index { get; private set; }

        public string? ObservableMethodName { get; private set; }

        public string Path { get; }

        public IExpressionNode? Expression { get; }

        #endregion

        #region Implementation of interfaces

        public abstract object? GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path);

        public abstract object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata);

        public IBindingMemberExpressionNode Update(int index, EnumFlags<BindingMemberExpressionFlags> flags, EnumFlags<MemberFlags> memberFlags, string? observableMethodName)
        {
            if (Index == index && _flags == flags.Value() && _memberFlags == memberFlags.Value() && ObservableMethodName == observableMethodName)
                return this;

            var expression = Clone(Metadata);
            expression._flags = flags.Value();
            expression._memberFlags = memberFlags.Value();
            expression.Index = index;
            expression.ObservableMethodName = observableMethodName;
            return expression;
        }

        #endregion

        #region Methods

        protected override bool Equals(TExpression other, IExpressionEqualityComparer? comparer) =>
            _flags == other._flags && _memberFlags == other._memberFlags && Index == other.Index && Path.Equals(other.Path) &&
            string.Equals(ObservableMethodName, other.ObservableMethodName) && Equals(Expression, other.Expression, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer)
        {
            if (Expression == null)
                return HashCode.Combine(hashCode, Index, Path, _flags, _memberFlags, ObservableMethodName);
            return HashCode.Combine(hashCode, Index, Path, _flags, _memberFlags, ObservableMethodName, Expression.GetHashCode(comparer));
        }

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => this;

        protected MemberPathObserverRequest GetObserverRequest(string path, IReadOnlyMetadataContext? metadata) =>
            new(MugenService.ObservationManager.GetMemberPath(path, metadata), MemberFlags,
                Flags.HasFlag(BindingMemberExpressionFlags.ObservableMethods) ? ObservableMethodName : null, Flags.HasFlag(BindingMemberExpressionFlags.StablePath),
                Flags.HasFlag(BindingMemberExpressionFlags.Observable), Flags.HasFlag(BindingMemberExpressionFlags.StablePath), this);

        protected string MergePath(string value)
        {
            if (string.IsNullOrEmpty(Path))
                return value;
            if (Path.StartsWith("[", StringComparison.Ordinal))
                return value + Path;
            return value + "." + Path;
        }

        public override string ToString()
        {
            if (Expression == null)
                return $"bind{Index}({Path})";
            return $"bind{Index}({Path}, {Expression})";
        }

        #endregion
    }
}