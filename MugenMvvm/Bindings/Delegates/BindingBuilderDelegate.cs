using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;

namespace MugenMvvm.Bindings.Delegates
{
    public delegate ItemOrArray<BindingExpressionRequest> BindingBuilderDelegate<TTarget, TSource>(BindingBuilderTarget<TTarget, TSource> target)
        where TTarget : class
        where TSource : class;
}