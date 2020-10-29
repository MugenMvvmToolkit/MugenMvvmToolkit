using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestExpressionConverterComponent<T> : IExpressionConverterComponent<T>, IHasPriority where T : class
    {
        #region Properties

        public int Priority { get; set; }

        public Func<IExpressionConverterContext<T>, T, IExpressionNode?>? TryConvert { get; set; }

        #endregion

        #region Implementation of interfaces

        IExpressionNode? IExpressionConverterComponent<T>.TryConvert(IExpressionConverterContext<T> context, T expression) => TryConvert?.Invoke(context, expression);

        #endregion
    }
}