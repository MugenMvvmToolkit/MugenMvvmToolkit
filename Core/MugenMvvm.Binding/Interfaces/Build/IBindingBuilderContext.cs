using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;

namespace MugenMvvm.Binding.Interfaces.Build
{
    public interface IBindingBuilderContext<out TTarget>
        where TTarget : class
    {
        [BindingMacros(MacrosConstant.Binding)]
        IBinding Binding { get; }

        [BindingMacros(MacrosConstant.Target)]
        TTarget Target { get; }

        [BindingMacros(MacrosConstant.EventArgs)]
        T EventArgs<T>();
    }

    public interface IBindingBuilderContext<out TTarget, out TSource> : IBindingBuilderContext<TTarget>
        where TTarget : class
        where TSource : class
    {
        [BindingMacros(MacrosConstant.Source)]
        TSource Source { get; }
    }
}