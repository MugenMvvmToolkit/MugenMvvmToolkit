using System;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class TestExpressionConverterComponent<T> : IExpressionConverterComponent<T>, IHasPriority where T : class
    {
        #region Properties

        public int Priority { get; set; }

        public Func<IExpressionConverterContext<T>, T, IExpressionNode?>? TryConvert { get; set; }

        #endregion

        #region Implementation of interfaces

        IExpressionNode? IExpressionConverterComponent<T>.TryConvert(IExpressionConverterContext<T> context, T expression)
        {
            return TryConvert?.Invoke(context, expression);
        }

        #endregion
    }
}