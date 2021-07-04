﻿using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class CompiledExpressionCompiler : IExpressionCompilerComponent, IHasPriority
    {
        public int Priority { get; init; } = CompilingComponentPriority.LinqCompiler;

        public ICompiledExpression TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata) =>
            new CompiledExpression(expression, metadata) {ExpressionBuilders = compiler.GetComponents<IExpressionBuilderComponent>(metadata)};
    }
}