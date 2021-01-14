using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Core;

namespace MugenMvvm.Bindings.Interfaces.Build
{
    public interface IBindingBuilderContext
    {
        [BindingMacros(MacrosConstant.Binding)]
        IBinding Binding { get; }

        [BindingMacros(MacrosConstant.EventArgs)]
        T EventArgs<T>();
    }

    public interface IBindingBuilderContext<out TTarget> : IBindingBuilderContext
        where TTarget : class
    {
        [BindingMacros(MacrosConstant.Target)]
        TTarget Target { get; }
    }

    public interface IBindingBuilderContext<out TTarget, out TSource> : IBindingBuilderContext<TTarget>
        where TTarget : class
        where TSource : class
    {
        [BindingMacros(MacrosConstant.Source)]
        TSource Source { get; }
    }
}