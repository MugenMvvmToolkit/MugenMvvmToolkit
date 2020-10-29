using MugenMvvm.Bindings.Build;
using MugenMvvm.Bindings.Parsing;

namespace MugenMvvm.Bindings.Delegates
{
    public delegate BindingExpressionRequest BindingBuilderDelegate<TTarget, TSource>(BindingBuilderTarget<TTarget, TSource> target)
        where TTarget : class
        where TSource : class;
}