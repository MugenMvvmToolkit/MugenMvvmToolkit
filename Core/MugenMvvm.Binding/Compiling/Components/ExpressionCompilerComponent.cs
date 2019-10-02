using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public class ExpressionCompilerComponent : IExpressionCompilerComponent, IHasPriority
    {
        #region Fields

        private readonly ComponentTracker<ICompiler, IExpressionCompiler> _componentTracker;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        public ExpressionCompilerComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _componentTracker = new ComponentTracker<ICompiler, IExpressionCompiler>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return Compile(expression, metadata);
        }

        #endregion

        #region Methods

        protected virtual ICompiledExpression Compile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return new LambdaCompiledExpression(this, expression, metadata);
        }

        #endregion

        #region Nested types

        private readonly struct CacheKey
        {
            #region Fields

            public readonly Type?[]? Types;
            public readonly object?[]? Values;

            #endregion

            #region Constructors

            public CacheKey(object?[] values)
            {
                Values = values;
                Types = null;
            }

            private CacheKey(Type?[] types)
            {
                Types = types;
                Values = null;
            }

            #endregion

            #region Methods

            public CacheKey ToTypeKey()
            {
                if (Types != null)
                    return this;

                var types = new Type[Values.Length];
                for (var i = 0; i < Values.Length; i++)
                    types[i] = Values[i]?.GetType();
                return new CacheKey(types);
            }

            #endregion
        }

        private sealed class LambdaCompiledExpression : LightDictionary<CacheKey, Func<object?[], object?>>, ICompiledExpression, IContext
        {
            #region Fields

            private readonly ExpressionCompilerComponent _compiler;
            private readonly IExpressionNode _expression;
            private IReadOnlyMetadataContext? _metadata;

            private object?[] _values;

            #endregion

            #region Constructors

            public LambdaCompiledExpression(ExpressionCompilerComponent compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata) : base(3)
            {
                _compiler = compiler;
                _expression = expression;
                _metadata = metadata;
            }

            #endregion

            #region Properties

            public bool HasMetadata => _metadata != null;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata is IMetadataContext ctx)
                        return ctx;

                    Interlocked.CompareExchange(ref _metadata, _metadata.ToNonReadonly(this, _compiler._metadataContextProvider), null);
                    return (IMetadataContext)_metadata!;
                }
            }

            #endregion

            #region Implementation of interfaces

            public object? Invoke(object?[] values, IReadOnlyMetadataContext metadata)
            {
                return null;
                //                if (_values == null)
                //                    _values = new object[values.Length + 1];
                //
                //                for (int i = 0; i < values.Length; i++)
                //                    _values[i + 1] = values[i];
                //                _values[_values.Length - 1] = metadata;
                //
                //                var key = new CacheKey(values);
                //                if (!TryGetValue(key, out var func))
                //                {
                //                    func = Compile();
                //                    this[key.ToTypeKey()] = func;
                //                }
                //
                //                var result = func(_values);
                //                Array.Clear(_values, 0, _values.Length);
                //                return result;
            }

            public Expression Compile(IExpressionNode expression)
            {
                var components = _compiler._componentTracker.GetComponents();
                foreach (var component in components)
                {
                    var compile = component.TryCompile(this, expression);
                    if (compile != null)
                        return compile;
                }

                BindingExceptionManager.ThrowCannotCompileExpression(expression);
                return null!;
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                var xTypes = x.Types;
                var yTypes = y.Types;
                if (xTypes.Length != yTypes.Length)
                    return false;
                for (var i = 0; i < xTypes.Length; i++)
                {
                    if (xTypes[i] != yTypes[i])
                        return false;
                }

                return true;
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    var hash = 0;
                    if (key.Types == null)
                    {
                        for (var index = 0; index < key.Values.Length; index++)
                        {
                            var type = key.Values[index]?.GetType();
                            hash ^= type == null ? 0 : type.GetHashCode() * 397;
                        }
                    }
                    else
                    {
                        for (var index = 0; index < key.Types.Length; index++)
                        {
                            var type = key.Types[index];
                            hash ^= type == null ? 0 : type.GetHashCode() * 397;
                        }
                    }


                    return hash;
                }
            }

            #endregion
        }

        public interface IContext : IMetadataOwner<IMetadataContext>
        {
            MethodInfo? GetCurrentLambdaMethod();

            void SetCurrentLambdaType(Type? lambdaType);

            ParameterExpression? GetParameterExpression(IParameterExpression parameterExpression);

            void SetParameterExpression(IParameterExpression parameterExpression, ParameterExpression? value);

            Expression Compile(IExpressionNode expression);
        }

        public interface ICompiler : IComponent<IExpressionCompiler>
        {
            Expression? TryCompile(IContext context, IExpressionNode expression);
        }

        #endregion
    }
}