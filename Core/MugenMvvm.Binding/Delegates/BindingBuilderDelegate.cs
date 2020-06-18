using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Parsing;

namespace MugenMvvm.Binding.Delegates
{
    public delegate ExpressionConverterRequest BindingBuilderDelegate<TTarget, TSource>(BindingBuilderTarget<TTarget, TSource> target)
        where TTarget : class
        where TSource : class;
}