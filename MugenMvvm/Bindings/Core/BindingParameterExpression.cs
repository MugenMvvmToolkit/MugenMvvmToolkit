using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingParameterExpression : IEquatable<BindingParameterExpression>
    {
        private readonly ICompiledExpression? _compiledExpression;
        private readonly object? _value;

        public BindingParameterExpression(object? value, ICompiledExpression? compiledExpression)
        {
            if (value is IBindingMemberExpressionNode[])
                Should.NotBeNull(compiledExpression, nameof(compiledExpression));
            _value = value;
            _compiledExpression = compiledExpression;
        }

        [MemberNotNullWhen(false, nameof(_value), nameof(_compiledExpression))]
        public bool IsEmpty => _value == null && _compiledExpression == null;

        public BindingParameterValue ToBindingParameter(object target, object? source, IReadOnlyMetadataContext? metadata) =>
            new(BindingMugenExtensions.ToBindingSource(_value, target, source, metadata), _compiledExpression);

        public bool Equals(BindingParameterExpression other) => Equals(_compiledExpression, other._compiledExpression) && Equals(_value, other._value);

        public override bool Equals(object? obj) => obj is BindingParameterExpression other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_compiledExpression, _value);
    }
}