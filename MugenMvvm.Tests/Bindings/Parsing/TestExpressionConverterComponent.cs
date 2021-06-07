﻿using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Parsing
{
    public class TestExpressionConverterComponent<T> : IExpressionConverterComponent<T>, IHasPriority where T : class
    {
        public Func<IExpressionConverterContext<T>, T, IExpressionNode?>? TryConvert { get; set; }

        public int Priority { get; set; }

        IExpressionNode? IExpressionConverterComponent<T>.TryConvert(IExpressionConverterContext<T> context, T expression) => TryConvert?.Invoke(context, expression);
    }
}