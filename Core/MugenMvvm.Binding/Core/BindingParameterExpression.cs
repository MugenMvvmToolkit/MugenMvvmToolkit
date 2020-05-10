using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingParameterExpression
    {
        #region Fields

        private readonly ICompiledExpression? _compiledExpression;
        private readonly object? _value;

        #endregion

        #region Constructors

        public BindingParameterExpression(object? value, ICompiledExpression? compiledExpression)
        {
            if (value is IBindingMemberExpressionNode[])
                Should.NotBeNull(compiledExpression, nameof(compiledExpression));
            _value = value;
            _compiledExpression = compiledExpression;
        }

        #endregion

        #region Properties

        public bool IsEmpty => _value == null && _compiledExpression == null;

        #endregion

        #region Methods

        public BindingParameterValue ToBindingParameter(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (_value is IBindingMemberExpressionNode v)
            {
                var observer = v.GetBindingSource(target, source, metadata);
                return new BindingParameterValue(observer, _compiledExpression);
            }

            if (_value is IBindingMemberExpressionNode[] nodes)
            {
                var observers = new object?[nodes.Length];
                for (var i = 0; i < nodes.Length; i++)
                    observers[i] = nodes[i].GetBindingSource(target, source, metadata);
                return new BindingParameterValue(observers, _compiledExpression);
            }

            return new BindingParameterValue(_value, _compiledExpression);
        }

        #endregion
    }
}